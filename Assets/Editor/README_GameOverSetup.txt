Instruções para configurar o Game Over HUD:

1) Criar o painel GameOver:
   - Hierarchy > Create > UI > Canvas (se não tiver um Canvas na cena).
   - Dentro do Canvas: Create > UI > Panel. Renomeie para `GameOverPanel`.
   - Ajuste o RectTransform do painel para centralizar e dar o visual desejado.

2) Conteúdo do painel (exemplo mínimo):
   - `Title` (TextMeshPro - `TextMeshPro - Text`): texto grande "Game Over".
   - `Message` (TextMeshPro): texto secundário para mostrar motivo/score.
   - Botões (UI > Button ou TextMeshPro Button): `Retry`, `MainMenu`, `Quit`.

3) Adicionar o manager:
   - Crie um GameObject vazio na cena, renomeie para `GameOverManager`.
   - Anexe o script `GameOverManager` (Assets/Scripts/Common/GameOverManager.cs).
   - No Inspector do `GameOverManager`, arraste o `GameOverPanel` para o campo `Game Over Panel`.
   - Arraste o `Title` TextMeshPro para `Title Text` (opcional).
   - Arraste o `Message` TextMeshPro para `Message Text` (opcional).

4) Conectar os botões:
   - `Retry` button: On Click() -> + -> arraste `GameOverManager` -> escolha `GameOverManager.Retry()`.
   - `MainMenu` button: On Click() -> + -> arraste `GameOverManager` -> escolha `GameOverManager.GoToMainMenu()`.
   - `Quit` button: On Click() -> + -> arraste `GameOverManager` -> escolha `GameOverManager.QuitGame()`.

5) Comportamento:
   - Quando `PlayerThird.Die()` for chamado, o script tentará encontrar o `GameOverManager` na cena e chamar `ShowGameOver()`.
   - Se não houver `GameOverManager`, o jogo apenas pausará e liberará o cursor.

6) Observações:
   - Coloque o `GameOverManager` na cena Phase1.
   - Ajuste `mainMenuSceneIndex` no `GameOverManager` caso o índice da cena de menu principal seja diferente.

Se quiser, eu posso criar automaticamente um `GameOverPanel` por código para testes rápidos — quer que eu crie um painel gerado em tempo de execução?