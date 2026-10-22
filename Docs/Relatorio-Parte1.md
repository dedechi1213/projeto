# Crypt Survivor — relatório da Parte 1

Implementado no projeto existente, Unity 2022.3.40f1, Windows, Input Manager clássico. A SampleScene original foi preservada. O documento “Projeto Sobrevivência 2D.html” não foi localizado; a implementação seguiu o pedido detalhado anexado.

## Entrega

Abra Assets/Scenes/GameArena.unity e pressione Play. Movimento: WASD ou setas. Mira: cursor. Disparo contínuo: mantenha o botão esquerdo pressionado. Após morrer, interrompa e reinicie Play para recomeçar. Não há ondas, placar, barra de vida nem tela de Game Over nesta etapa.

- Arena com piso e paredes em Tilemaps, quatro limites com BoxCollider2D, sarcófagos decorativos sem bloquear a circulação, câmera ortográfica e cinco posições válidas de nascimento.
- Prefabs Player, Skeleton e Projectile, com componentes e referências conectados; cinco esqueletos de teste já posicionados.
- Movimento em oito direções, diagonal limitada à velocidade cardinal, Rigidbody2D sem gravidade e rotação bloqueada.
- Sprites PNG selecionados dos dois pacotes, filtro Point, sem compressão ou mipmaps, spritesheets recortadas e controladores Animator. Idle, Walk e Attack em oito direções; Run e Roll reservados sem mecânicas adicionais. Esqueleto com Idle, Walk e Death.
- Disparo com velocidade, dano, frequência e tempo de vida configuráveis. Impacto em inimigos e paredes, descarte após impacto/expiração e exclusão do jogador.
- Vida inicial 100, eventos HealthChanged e Died, controles desativados ao morrer e notificação ao GameManager.
- Perseguição física, dano de contato com intervalo, vida configurável, animação de morte, remoção após morte e evento EnemyHealth.EnemyDied preparado para pontuação.

## Arquivos criados ou alterados

- Assets/Scenes/GameArena.unity e metadados.
- Assets/Prefabs/{Player,Skeleton,Projectile}.prefab e metadados.
- Assets/Scripts/Player/{PlayerMovement,PlayerAnimationController,PlayerAttack,PlayerHealth}.cs.
- Assets/Scripts/Enemies/{EnemyController,EnemyHealth}.cs.
- Assets/Scripts/Combat/Projectile.cs.
- Assets/Scripts/Managers/{GameManager,DepthSort}.cs.
- Assets/Editor/CryptArenaBuilder.cs: montagem inicial com proteção para preservar a cena existente.
- Assets/Editor/CryptValidation.cs: validação reproduzível em Play Mode e renderização de prévia pelo menu Crypt Survivor.
- Assets/Animations: controladores Player/Skeleton e 43 clipes.
- Assets/Art/Player: cinco spritesheets necessárias; Assets/Art/Crypt: piso, parede, sarcófago, três animações de esqueleto e projétil. Metadados incluem configurações e recortes.
- Assets/Tilemaps: quatro tiles de piso e um de parede.
- Pastas Scripts/UI e Audio preparadas para expansão.
- ProjectSettings/EditorSettings.asset: modo 2D; ProjectSettings/EditorBuildSettings.asset: GameArena como cena inicial.
- Docs/GameArena.png, Docs/Validacao-Parte1.txt e este relatório.

## Testes efetivamente realizados

18 verificações automatizadas aprovadas em Play Mode na Unity, com física real: vida inicial, quantidade de inimigos, Tilemaps, perseguição, impacto e dano de projétil, remoção no impacto, proteção contra dano ao próprio jogador, desativação do inimigo morto, execução da animação Death, evento único de morte, rejeição de dano após morte, limite da parede, remoção do cadáver, dano por contato, intervalo de contato, expiração do projétil, eventos de vida/morte e comunicação com GameManager. Resultados em Validacao-Parte1.txt.

Prévia 1280 × 720 renderizada pela câmera da Unity e inspecionada visualmente. O InputManager foi inspecionado: Horizontal usa A/D e esquerda/direita; Vertical usa W/S e cima/baixo. Os testes de disparo acionam a mesma função Fire usada pelo mouse.

Não foi realizada uma sessão manual completa com teclado e mouse físicos nem teste em outro computador. Sensação dos controles e correspondência visual de todas as direções ainda merecem conferência manual. As animações do personagem e da cripta têm estilos distintos nos próprios pacotes fornecidos.

## Escopo futuro

Parte 2 permanece pendente: três ondas definitivas, ScoreManager, barra de vida e Game Over. Não foram implementadas funcionalidades dessa etapa.

## Compilação Windows

Build StandaloneWindows64 concluído com resultado Succeeded. Executável: Build/Crypt Survivor.exe. Distribua a pasta Build inteira, incluindo Crypt Survivor_Data e as bibliotecas. O executável não foi testado manualmente fora do Editor.
