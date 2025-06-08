# 🃏 BlackJack Multiplayer (Trabalho Prático de Redes)

Este projeto implementa uma versão simplificada do jogo **BlackJack** utilizando **comunicação via UDP** entre **cliente e servidor**.

O objetivo é aplicar na prática os conceitos de redes de computadores, como sockets, controle de estados e comunicação com mensagens estruturadas (JSON).

---

## Tecnologias Utilizadas

- Linguagem: **C# (.NET 6)**
- Comunicação: **UDP (Sockets)**
- Serialização: **System.Text.Json**
- Interface: **Console (cliente e servidor)**

---

## Como Executar o Projeto

### Pré-requisitos

- [.NET SDK 6.0+](https://dotnet.microsoft.com/en-us/download)

### 1. Clone o repositório

```bash
git clone https://github.com/NayaraFJ/redes-blackjack.git
cd redes-blackjack
```

### 2. Compine no Visual Studio

```bash
Compine a solução inteira usando o Visual Studio
```

### 3. Execute o **Servidor**

```bash
Execute o servidor na pasta .\redes-blackjack\blackjack-servidor\bin\Debug\net8.0\blackjack-servidor.exe
```

> O servidor será iniciado na porta **9000/UDP**

### 4. Execute o **Cliente** 

```bash
Execute o cliente na pasta .\redes-blackjack\blackjack-cliente\bin\Debug\net8.0\blackjack-cliente.exe
```

Você será solicitado a digitar o nome do jogador.

Repita esse passo para adicionar mais de um jogador ao jogo.

---

## 🗂️ Estrutura do Projeto

```
📁 redes-blackjack                        # Repositório principal do projeto
├── 📁 blackjack-cliente                 # Projeto do cliente (jogador)
│   └── Cliente.cs                       # Aplicação console do cliente
│
├── 📁 blackjack-servidor                # Projeto do servidor (gerencia o jogo)
│   └── Servidor.cs                      # Aplicação console do servidor
│
└── 📁 blackjack_interface               # Projeto compartilhado (entidades e enums)
    ├── Carta.cs                         # Representa uma carta do baralho
    ├── Baralho.cs                       # Lógica para criar e embaralhar cartas
    ├── Jogador.cs                       # Representa um jogador e sua pontuação
    ├── Mensagem.cs                      # Estrutura de mensagem trocada via UDP
    ├── 📁 Enumerator                    # Subpasta contendo enums e comandos
    │   ├── Comandos.cs                  # Enum com os comandos de comunicação
    │   └── Naipe.cs                     # Enum com os naipes (copas, paus, etc.)

```

---

## Funcionalidades

- Entrada de múltiplos jogadores antes da rodada
- Regras do BlackJack implementadas
- Rodadas com cartas únicas (sem repetição)
- Encerramento automático da rodada
- Envio de resultado e reinício automático
- Tratamento de desconexão e inatividade
- Comunicação toda feita via JSON

---

## Participantes

- Nayara [@NayaraFJ](https://github.com/seu-usuario)

---

## 📄 Licença

Este projeto é apenas para fins educacionais na disciplina de Redes de Computadores.
