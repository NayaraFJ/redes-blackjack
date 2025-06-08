# 🃏 BlackJack Multiplayer (Trabalho Prático de Redes)

Este projeto implementa uma versão simplificada do jogo **BlackJack** utilizando **comunicação via UDP** entre **cliente e servidor**.

O objetivo é aplicar na prática os conceitos de redes de computadores, como sockets, controle de estados e comunicação com mensagens estruturadas (JSON).

---

## 🧠 Tecnologias Utilizadas

- Linguagem: **C# (.NET 6)**
- Comunicação: **UDP (Sockets)**
- Serialização: **System.Text.Json**
- Interface: **Console (cliente e servidor)**

---

## 🚀 Como Executar o Projeto

### ✅ Pré-requisitos

- [.NET SDK 6.0+](https://dotnet.microsoft.com/en-us/download)

### 🖥️ 1. Clone o repositório

```bash
git clone https://github.com/seu-usuario/blackjack-redes.git
cd blackjack-redes
```

### 🖧 2. Execute o **Servidor**

```bash
cd Servidor
dotnet run
```

> O servidor será iniciado na porta **9000/UDP**

### 🎮 3. Execute o **Cliente** (em outro terminal ou máquina)

```bash
cd Cliente
dotnet run
```

Você será solicitado a digitar o nome do jogador.

Repita esse passo para adicionar mais de um jogador ao jogo.

---

## 🗂️ Estrutura do Projeto

```
📁 blackjack-redes
 ├── 📁 Cliente        # Aplicação console para os jogadores
 ├── 📁 Servidor       # Aplicação console do servidor do jogo
 └── 📁 blackjack_interface
      ├── Carta.cs
      ├── Baralho.cs
      ├── Jogador.cs
      ├── Mensagem.cs
      ├── Comandos.cs
      └── Naipe.cs
```

---

## 💡 Funcionalidades

- Entrada de múltiplos jogadores antes da rodada
- Regras do BlackJack implementadas
- Rodadas com cartas únicas (sem repetição)
- Encerramento automático da rodada
- Envio de resultado e reinício automático
- Tratamento de desconexão e inatividade
- Comunicação toda feita via JSON

---

## 👩‍💻 Participantes

- Nayara [@seu-usuario](https://github.com/seu-usuario)
- [Segundo integrante, se houver]

---

## 📄 Licença

Este projeto é apenas para fins educacionais na disciplina de Redes de Computadores. Direitos reservados aos autores.
