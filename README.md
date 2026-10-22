# Crypt Survivor

**Estudante:Tiago de Oliveira Silva**

Protótipo 2D de ação e sobrevivência em uma cripta medieval. Sobreviva a três ondas, elimine 25 esqueletos e conquiste até **2.500 pontos**.

## Executar

- Unity **2022.3.40f1**, projeto 2D, C#, Windows, Input Manager clássico.
- Adicione esta pasta no Unity Hub e abra com a versão indicada. Aguarde a importação.
- Abra `Assets/Scenes/MainMenu.unity` e pressione **Play**. Clique **JOGAR**.
- Também é possível abrir `GameArena.unity` para iniciar diretamente uma partida.
- Build Windows: execute `Build/Crypt Survivor.exe`, quando disponível. Mantenha a pasta `_Data`, MonoBleedingEdge e DLLs junto ao executável. A pasta Build não é incluída no Git.
- Para compilar: File → Build Settings → Windows / x86_64. As cenas MainMenu e GameArena já estão configuradas nessa ordem.

## Controles

| Ação | Controle |
|---|---|
| Movimento em oito direções | WASD ou setas |
| Mira | Cursor do mouse |
| Disparo | Botão esquerdo; segure para disparar continuamente |
| Reinício e retorno ao menu | Botões dos painéis finais |

O botão **SAIR** fecha a aplicação na build e interrompe o modo Play no Editor.

## Funcionalidades

- Menu inicial com botões, arena com Tilemaps, paredes físicas e decorações.
- Movimento com diagonal normalizada, animações direcionais Idle/Walk/Attack e projéteis.
- Vida inicial 100, dano por contato com intervalo e feedback visual de dano.
- Esqueletos com perseguição, vida, animação de morte e remoção.
- Exatamente três ondas: **5, 8 e 12** inimigos, intervalo entre ondas e nascimento em posições válidas longe do jogador.
- Pontuação de **100 por inimigo**, sem contagem duplicada.
- HUD com Canvas Scaler e TextMeshPro: barra e número da vida, pontuação, onda e anúncios temporários.
- Vitória, Game Over, pontuação final, nova partida e retorno ao menu.
- Estados Playing/Victory/GameOver, bloqueio dos sistemas após encerramento e reinício completo.
- Efeitos sonoros sintetizados para disparo, dano, morte, vitória e derrota, sem arquivos externos adicionais.
- Câmera adapta o enquadramento à proporção da janela para manter a arena inteira visível.
- Prefabs e eventos preparados para extensão. Run/Roll importados e reservados, sem novas mecânicas.

## Estrutura

`Assets/Art`, `Animations`, `Prefabs`, `Scenes`, `Scripts/{Player,Enemies,Combat,Managers,UI}`, `Tilemaps` e `Audio`.

A cena SampleScene da base foi preservada, mas não faz parte da build. Os cinco inimigos fixos de teste da Parte 1 foram substituídos pela geração da primeira onda, com o mesmo prefab e combate.

## Verificações

- Menu **Crypt Survivor → Run complete game validation**: valida o fluxo das três ondas, pontos, HUD, vitória, derrota, reinício e navegação em Play Mode. Essa rotina utiliza dano programático para testar as transições; não representa uma partida manual completa.
- Menu **Crypt Survivor → Run combat validation**: testes de física e combate da Parte 1.
- Resultados e prévias em `Docs/`. Consulte `Docs/Relatorio-Parte2.md` para os testes efetivamente executados e as limitações.

## Pendências e limites

- Preencher o nome do estudante.
- Confirmar autoria e termos de licença dos dois pacotes antes de distribuir seus arquivos ou tornar o repositório público. Os ZIPs fornecidos não incluem uma licença explícita.
- Não há inventário, equipamentos, multiplayer, habilidades adicionais ou mecânicas de Run/Roll; estão fora do escopo.

## Créditos e licenças

| Recurso | Uso e origem | Licença |
|---|---|---|
| Top-Down Asset Pack (Sept 2026) | PNGs de personagem e animações, ZIP fornecido pelo estudante | Autoria e licença não informadas no ZIP; manter os termos da aquisição original |
| EPIC RPG World Pack - Crypt V.1.6 | Tiles de cripta, sarcófago, esqueleto e projétil, ZIP fornecido pelo estudante | Autoria e licença não informadas no ZIP; manter os termos da aquisição original |
| TextMeshPro / Liberation Sans | Interface; recursos essenciais do pacote oficial Unity instalado | Consulte `Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt` e os avisos do pacote TextMeshPro |

Não se presume que os pacotes externos tenham licença livre. A ausência de um arquivo de licença nos ZIPs não autoriza redistribuição irrestrita. Os avisos e arquivos de licença fornecidos pelos recursos essenciais TextMeshPro foram preservados.

## Entrega e GitHub

O `.gitignore` exclui Library, Temp, Logs, Obj, builds e arquivos locais de IDE. Versione **Assets com todos os `.meta`**, Packages, ProjectSettings, README e Docs. Nenhum repositório foi publicado por esta implementação.

