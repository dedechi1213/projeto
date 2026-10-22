# Crypt Survivor — entrega da Parte 2

## Abrir e jogar

Projeto existente em `C:\unity\Meu projeto`, Unity **2022.3.40f1**. Abra pelo Unity Hub, aguarde a importação e abra `Assets/Scenes/MainMenu.unity`. Pressione Play e clique JOGAR. A GameArena também inicia uma partida diretamente. WASD/setas movem, mouse mira e botão esquerdo dispara continuamente. Os botões dos painéis finais reiniciam ou retornam ao menu. SAIR interrompe o Play no Editor e fecha a aplicação Windows.

## Funcionalidades implementadas

- Sequência fixa de três ondas com 5, 8 e 12 esqueletos. Uma onda só avança ao eliminar todos os seus inimigos; há 2,5 segundos entre ondas, anúncios ONDA 1, ONDA 2 e ONDA FINAL e busca de posições livres a pelo menos 3 unidades do jogador.
- Pontuação começa em zero, soma exatamente 100 por morte válida e alcança 2500 ao vencer. Proteções contra pontuação duplicada, mortes externas à onda e pontuação após encerramento.
- HUD Canvas/TextMeshPro com número e barra de vida, pontuação e onda; atualização por eventos e textos sem bloquear disparos. Canvas Scaler preparado para outras resoluções.
- MainMenu com fundo reutilizando os tiles da cripta, título e botões JOGAR/SAIR; EventSystem, Graphic Raycaster e eventos persistentes configurados nas cenas.
- Painéis de vitória e Game Over com mensagem, pontuação final, nova partida e retorno ao menu.
- GameManager com estados Playing, Victory e GameOver. O primeiro resultado encerra a partida e impede resultado concorrente. Controles, inimigos, ondas e projéteis param ao terminar.
- Reinício por recarga da cena: vida, ondas, inimigos, eventos e pontos reiniciados integralmente. Assinaturas dos eventos são removidas ao descarregar a cena.
- Feedback vermelho ao receber dano e sons simples sintetizados para ataque, dano, morte de esqueleto, vitória e derrota.
- Câmera adapta o enquadramento à proporção da janela para manter a arena visível.
- README, créditos com limites de licença conhecidos e .gitignore de Unity. Nenhum repositório foi publicado.

## Preservação da Parte 1

Arena, piso, paredes físicas, decorações, sprites, animações e os três prefabs foram mantidos. Os cinco esqueletos posicionados para teste foram removidos da cena para dar lugar aos cinco gerados pela primeira onda, usando o mesmo prefab. A SampleScene original permanece no projeto, fora da build. Movimento, disparos e perseguição continuam usando os scripts existentes.

## Arquivos criados e modificados

**Novos scripts:**

- `Assets/Scripts/Managers/WaveManager.cs`, `ScoreManager.cs`, `GameAudio.cs`, `ArenaCamera.cs`.
- `Assets/Scripts/UI/GameplayHUD.cs`, `MainMenuController.cs`.
- `Assets/Scripts/Combat/DamageFlash.cs`.
- `Assets/Editor/CryptGameBuilder.cs`, `CryptFullValidation.cs`, `CryptDeliveryAudit.cs`.

**Scripts integrados:**

- `Assets/Scripts/Managers/GameManager.cs`: estados, início, vitória/derrota e navegação.
- `Assets/Scripts/Player/PlayerHealth.cs`: bloqueio de dano após encerramento e feedback.
- `Assets/Scripts/Player/PlayerAttack.cs`: som de disparo.
- `Assets/Scripts/Enemies/EnemyHealth.cs`: bloqueio de dano após encerramento e feedback de dano/morte.
- `Assets/Editor/CryptValidation.cs`: compilação incluindo as duas cenas.

**Cenas, prefabs, recursos e configuração:**

- `Assets/Scenes/MainMenu.unity`: nova cena; `GameArena.unity`: gerenciadores, ondas, HUD, painéis e referências.
- `Assets/Prefabs/Player.prefab` e `Skeleton.prefab`: DamageFlash adicionado.
- `Assets/TextMesh Pro/`: recursos essenciais importados do pacote Unity já instalado, incluindo fonte e licença OFL.
- `ProjectSettings/EditorBuildSettings.asset`: MainMenu primeiro, GameArena segundo.
- `ProjectSettings/ProjectSettings.asset`: nome do jogo e janela Windows 1280×720.
- `.gitignore`, `README.md`, documentação e capturas em `Docs/`. Os arquivos Unity acompanham seus metadados `.meta`.

## Testes e evidências

As rotinas são executadas na Unity em Play Mode, com física real. O teste de fluxo elimina inimigos por chamadas de dano para verificar transições e pontuação sem depender de uma partida manual. Os botões são acionados pelo manipulador de clique do EventSystem; o raycast verifica que o mouse alcança cada botão.

- `Docs/Validacao-Parte2.txt`: **45 verificações aprovadas**, sem falhas; resultados da sequência completa, HUD, contagem, intervalos, vitória, derrota, navegação e reinícios. Também verifica ausência de erros de execução capturados durante a rotina.
- `Docs/Validacao-Combate-Final.txt`: **18 verificações aprovadas**, sem falhas; regressão dos 18 testes de física e combate da Parte 1 após integração.
- `Docs/Auditoria-Final.txt`: referências, cenas, controladores Animator, projétil e ausência de Missing Scripts.
- `Docs/MainMenu.png`, `Gameplay.png`, `Victory.png`, `GameOver.png`: prévias renderizadas e inspecionadas visualmente. Há também capturas 800×600 para conferir a adaptação da interface e do enquadramento.
- `Docs/Build-Windows-Parte2.txt`: compilação final **Succeeded**, Windows x86_64, com as duas cenas.
- `Docs/Teste-Build-Windows.txt`: executável aberto, inicialização sem erros identificados no Player.log e encerramento por Alt+F4. A captura da janela nativa falhou por timeout; o clique SAIR na build permanece sem teste manual.

As rotinas podem ser repetidas pelos menus `Crypt Survivor → Run complete game validation`, `Run combat validation` e `Audit scene and prefab references`. Execute uma rotina por vez, com alterações salvas; recompilar scripts durante o Play interrompe a rotina.

## Problemas tratados

- Contagem de inimigos pertence à onda e ignora eventos externos; a pontuação registra cada identificador uma única vez.
- Vitória aguarda a conclusão dos assinantes do evento de morte antes de exibir o total final.
- Encerramento bloqueia novos danos e geração, desativa controles e remove projéteis existentes.
- Eventos são removidos ao trocar de cena, evitando duplicação de pontos após reinícios.
- Enquadramento 4:3 foi ajustado para evitar esconder as laterais da arena.

## Pendências e limites

Preencher o nome do estudante e confirmar autoria/termos dos pacotes externos: os ZIPs não contêm uma licença explícita. O README não atribui uma licença livre aos assets. As licenças fornecidas pelo TextMeshPro foram preservadas.

Não foi feita uma partida manual completa usando teclado e mouse físicos nem uma avaliação auditiva dos sons. Sensação dos controles, equilíbrio da dificuldade e volume final merecem essa conferência. A verificação automatizada de botões não substitui um teste manual completo em outro computador. Run/Roll permanecem reservados, conforme escopo; não foram adicionados inventário, equipamentos, multiplayer ou novas habilidades.

## Entrega para GitHub

Versionar Assets (incluindo .meta), Packages, ProjectSettings, README, .gitignore e Docs. Library, Temp, Logs, Obj, UserSettings e Build são ignorados. Não foi criado nem publicado um repositório remoto. Para distribuir a build, envie a pasta Build inteira, mantendo o executável, a pasta `_Data`, MonoBleedingEdge e DLLs juntos.

