using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
//O require component faz o seguinte: Se por acaso eu esqueci de colocar algum componente, com esse Require o Unity adiciona automaticamente para mim. Nesse caso, o script exige NavMeshComponent, e se este nao existir, vai adicionar automaticamente.
public class EnemyBasicMovement : MonoBehaviour
{
    //Esse aqui é pesadão. Puta que pariu.

    public enum EnemyState { Patrol, Chase, Searching } //Eu poderia fazer de quinhentos jeitos. Mas esse é bem interessante: O inimigo se comporta em tres fases, a fase de patrulha, onde ele só segue os vetores de patrulha; 
                                                        //a fase de perseguição que ele só tem um alvo, que é o transform do player(aqui está como target); e a fase de procura, que é quando nós como player conseguimos fugir,
    public EnemyAttack enemyAttack;                     //ai a IA tenta buscar a gente por um tempo curto. Se esse tempo acabar, e a ultima posição do player não bater com a posição que o player realmente esta, a busca acaba
                                                        // e voltamos para a patrulha.
    public EnemyState currentState = EnemyState.Patrol; //O jogo começa com os inimigos em patrulha, somente.

    public Transform[] patrolPoints;
    private int patrolIndex = 0; //Como vamos somar o index sem um laço de repetição, criamos uma variavel para ser index do vetor.

    public Transform target; //transform do player
    public float chaseDistance = 15f; //auto explicativo
    public float lostTargetTime = 5f; //auto explicativo
    private Vector3 lastSeenPosition; //auto explicativo

    private float timerSinceLost = 0f; //variavel de contador.

    public float updateSpeed = 0.1f; // usada nos IEnumerator: Funcoes que podem ser pausadas ou retomadas quando quisermos.
    private WaitForSeconds wait; // variavel para dizer o tempo até pausar, ou o tempo até retomar a função, ou até parar ela de vez, etc.
    public Player player;

    private GameObject zona; // Essa zona é um gameobject dentro do player com um collider chamado "zona". Nos pegamos esse collider para dizer: Se eu estiver dentro dessa zona (se eu vi o player), eu diminuo a velocidade para ele poder fugir. 
                             //Se eu estiver com ele a vista, e eu não estou na zona, vou correr mais para ele não fugir. 
    private NavMeshAgent agent; // auto explicativo
    private Animator anim; // auto explicativo

    private float viewRadius = 10f; //o quao longe o inimigo enxerga (imagina um circulo, que o inimigo é o ponto central. O raio é o que conseguimos enxergar. 
    private float viewAngle = 200f; //Angulo de visao. Explicação depois.
    public LayerMask playerMask; //Layer que só o player tem. necessario pro raycast.
    public LayerMask obstructionMask; //Se essa layer tiver na frente do player, o inimigo nao enxerga a gente.

    private bool playerInSight = false; // auto explicativo
    private bool hasReachedLastPosition = false; // auto explicativo

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        zona = GameObject.FindGameObjectWithTag("Zona");
        anim = GetComponentInChildren<Animator>(); //o animator não esta no objeto que ESTE script esta, mas sim no filho dele.
        wait = new WaitForSeconds(updateSpeed);
    }

    void Start()
    {
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine() //Basicamente a maquina que organiza as funcoes de perseguir, buscar, e patrulhar.
    {
        while (enabled)
        {
            switch (currentState) //Escolha caso. Se eu estiver com o caso Patrol, eu chamo a função Patrol. E assim por diante.
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Chase:
                    if(!enemyAttack.isAttacking)
                    {
                            anim.SetInteger("state", 1);
                    }
                    Chase();
                    break;
                case EnemyState.Searching:
                    Search();
                    break;
            }
            yield return wait;
        }
    }
    private IEnumerator SearchAroundBeforePatrol() //Ato de buscar o personagem depois de ter perdido ele de vista. Depois de 2 segundos eu volto a patrulhar.
    {
        
        yield return new WaitForSeconds(2f);
        currentState = EnemyState.Patrol;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);

        if (EnemyCanSeePlayer()) //Se eu conseguir ver o player, playerInSight é verdade, e então eu vou persegui-lo. Como estou perseguindo no update, a ultima posição do player é igual a posição dele, e a atualização funciona.
        {
            playerInSight = true;
            currentState = EnemyState.Chase;
            timerSinceLost = 0f;

            lastSeenPosition = target.position;
            hasReachedLastPosition = false; //Nao at  ingi porque supomos que ainda estou perseguindo ele.
        }
        else if (playerInSight) //CASO EnemyCanSeePlayer retornar FALSO, eu venho para cá. Mais ainda: Se player insight por FALSO, eu vou passar a usar o contador, pois, aqui já perdemos o player de vista... 
                                //mas não desistimos ainda.
        {
            timerSinceLost += Time.deltaTime;
            if (timerSinceLost > lostTargetTime) //lostTargetTime é o tempo que o inimigo busca o player mesmo depois de perder ele de vista (ele ainda sabe aonde estamos).
                                                 //Se o contador for maior que esse tempo, entao o inimigo deixou de saber aonde estamos. Mas ainda busca a ultima vez que ele viu nós. Melhor dizendo, a ulima posição do player antes do contador
                                                 //acabar.
            {
                playerInSight = false;

                float dist = Vector3.Distance(player.transform.position, lastSeenPosition); //Se a distancia que estamos da IA de verdade for pequena, entao a IA tem uma determinação a mais e ainda busca a gente.
                                                                                            //Na vida real seria tipo: "Nao vejo ele, mas ainda escuto-o.". Em outras palavras, poderia ser um cooldown que a gente dá para a IA.
                if (dist < 3f)
                {
                    currentState = EnemyState.Searching;
                    
                    hasReachedLastPosition = false;
                    agent.SetDestination(lastSeenPosition);
                    anim.SetInteger("state", 4);

                }
                else
                {
                    currentState = EnemyState.Patrol; //A IA sabe que estamos longe, e então vai embora.
                    
                }

            }
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return; //Se o tamanho for zero, nao executa nada.

        agent.speed = player.moveSpeed - 3f; //Nesse caso o inimigo tem a nossa velocidade.

        if (!enemyAttack.isAttacking)
        {
            anim.SetInteger("state", 3);
        }


        if (!agent.pathPending && agent.remainingDistance < 0.5f) //agent.pathPending nos diz se ainda estamos calculando o caminho até o nosso destino. Se isso tá falso, então significa que chegamos no destino.
                                                                  //Aqui usamos o agent.remainingDistance pra dizer que nós já "chegamos" se estivermos a menos que 0.5f de distancia do destino.
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length; //Isso aqui é bem interessante, mas confuso. Somamos o index quando queremos ir para o proximo. O resto de divisao é importante porque, o unico caso que o resto dá zero é se nao me engane
                                                                   //quando estivermos no ultimo passo da patrulha. Ai a patrulha volta para o numero inicial novamente.
            agent.SetDestination(patrolPoints[patrolIndex].position);
        }
    }

    void Chase()
    {
        agent.SetDestination(target.position); //Pegamos o player.
        
        if (IsPlayerInZona())
        {
            agent.speed = player.moveSpeed + 2f;

        }
        else
        {
            agent.speed = player.SprintSpeed + 3f;

        }
    }

    void Search()
    {

        agent.speed = player.moveSpeed;
        anim.SetInteger("state", 4);

        if (!hasReachedLastPosition) //Isso aqui é uma preucaução. Se nao atingimos ainda a posição, vamos calcular a distancia. Se a distancia for pequena, atingimos ela e iniciamos SearchAroundBeforePatrol. 
        {
            float distance = Vector3.Distance(transform.position, lastSeenPosition);
            if (distance < 0.5f)
            {
                hasReachedLastPosition = true;
                StartCoroutine(SearchAroundBeforePatrol());
            }
        }
    }

    public bool IsPlayerInZona() //A esfera ao redor do inimigo vai pegar todos os colliders ao redor dele. Se tiver um collider com a tag zona, então o player ta na zona. E o metodo pode ser usado em outros cantos.
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 2f);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Zona"))
                return true;
        }
        return false;
    }

    bool EnemyCanSeePlayer() //Calculamos a direção do player. Nos normalizamos o vetor porque o raycast só gosta de um vetor para usar como DIRECAO. Ou seja, ele pode bugar se a gente usar um vetor com, sei la, (3, 0 ,4). Pode calcular errado.
                             //Como queremos só mira, é boa prática usar (1, 1, 1).
    {
        Vector3 directionToPlayer = (target.position - transform.position).normalized; //Direcao AO player.
        float distanceToPlayer = Vector3.Distance(transform.position, target.position); //Direção DO player. 

        if (distanceToPlayer < viewRadius) //Se a distancia do player for menor que o raio que expliquei lá em cima (se nao viu, é na declaracao dessa variavel viewRadius), vamos calcular o angulo entre o player e o inimigo.
        {
            float angleBetween = Vector3.Angle(transform.forward, directionToPlayer); //Calculamos suando Vector3.Angle.
            if (angleBetween < viewAngle / 2f)//Porque dividimos pela metade? Pra explicar, o Vector3.Angle retorna um angulo ENTRE DOIS VETORES. Os dois vetores em questao é o forward do INIMIGO, que sempre aponta para frente, e a direção ao player.
                                              //Se a gente nao dividir esse valor ao meio, o unity vai entender sem querer que o inimigo tem o dobro de campo de visão. Quando a gente divide por 2 um campo de visão que é de sei la, 120 graus, estamos
                                              //dizendo na verdade: O player está a 60 graus para a minha esquerda, ou 60 graus para a minha direita?
            {
                if (!Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstructionMask)) //Ai calculamos o raycast. Porque transform.position + vector3.up? Porque geralmente a gente quer calcular o raycast
                                                                                                                             //(num personagem humanoide) a partir dos olhos dele. E geralmente o transform.position fica no root (entre os pés), ou no
                                                                                                                             //centro do objeto. Assim, o rayscast pode passar por baixo e ver o player mesmo com a gente escondido, por uma parede, uma porta.
                                                                                                                             //Ai adicionamos Vector3.up para deixar o inicio do raycast na cabecinha do personagem. Uau.
                                                                                                                             //O "!" antes da linha é para dizer o seguinte: SE o raycast NÃO ESTA BATENDO num objeto da layer OBSTRUCTION MASK, entao
                                                                                                                             //o inimigo CONSEGUE ver a gente. Ou pode ver. Se estivermos longe não podemos (acima do valor viewRadius. Ou se estamos fora de
                                                                                                                             //viewAngle/2).
                {
                    return true;
                }
            }
        }
        return false;
    }
}


