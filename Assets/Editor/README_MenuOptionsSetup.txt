Instruções de configuração para o Menu de Opções (UI):

1) Criar um Canvas na cena (se não houver):
   - Hierarchy > Create > UI > Canvas
   - Certifique-se de que o Canvas Render Mode esteja em 'Screen Space - Overlay' e o Canvas Scaler em 'Scale With Screen Size'.

2) Criar o Painel de Opções:
   - Clique com o botão direito no Canvas > UI > Panel. Renomeie para 'OptionsPanel'.
   - Dentro do painel, crie Botões: Resume, Main Menu, Quit.
   - Ajuste o visual do painel para combinar com sua UI (fundo, layout).
   - Inicialmente desative o painel no Inspector (desmarque o GameObject) ou deixe ativado — o script irá escondê-lo no Start.

3) Adicionar o Manager:
   - Crie um GameObject vazio na cena, renomeie para 'MenuOptionsManager'.
   - Anexe o componente 'MenuOptionsManager' (script).
   - Arraste o GameObject 'OptionsPanel' para o campo 'Options Panel' do script no Inspector.
   - Configure 'phase1SceneName' e 'shopSceneName' caso os nomes das suas cenas sejam diferentes.

4) Conectar os botões:
   - Selecione o botão Resume -> On Click () -> + -> arraste MenuOptionsManager -> MenuOptionsManager.Resume()
   - Selecione o botão Main Menu -> On Click () -> + -> arraste MenuOptionsManager -> MenuOptionsManager.GoToMainMenu()
   - Selecione o botão Quit -> On Click () -> + -> arraste MenuOptionsManager -> MenuOptionsManager.QuitGame()

5) Comportamento do cursor:
   - O script libera e mostra o cursor enquanto o jogo estiver pausado, e tranca/oculta ao retomar.

6) Observações:
   - O script pausa o jogo definindo Time.timeScale = 0. Se você tiver áudio/música que deve pausar também, garanta que as fontes de áudio lidem com isso ou pause-as separadamente.
   - Se você usa nomes de classes diferentes para o controlador do jogador, adicione-os nas verificações de ShouldRespondInCurrentScene() no script.

Se quiser, posso modificar o script para suportar abrir o menu em outras cenas ou tornar o manager persistente entre cenas.