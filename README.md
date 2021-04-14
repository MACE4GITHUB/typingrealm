# TypingRealm

This project is the game where you need to type with your keyboard.

## Goals

The goal is to create a game that will be suitable for the following:

- training tool for learning touch typing
- multiplayer support to compete in typing skill

### Training tool

The difficulty of the game should be adaptable for the skill of the player. The
game should contain training elements that can facilitate learning touch typing.

### Multiplayer support

The game should contain multiplayer support so many people can compete against
each other.

## Design

The decision was made to make the game multiplayer-first and add support for
single player later on.

## How to run it

Now that we have initial version of the application ready, we can run it to
test the framework. There are two projects that we can run from the client
side:

- *TypingRealm.TestClient* - this is a test client that allows connecting to
the server and sending / receiving any messages.
- *TypingRealm.Chat.ConsoleApp* - this is a simple console Chat made with this
framework.

If you run Chat application, just enjoy chatting with friends and stress-test
the framework to find errors and fix it.

If you run TestClient application, currently it supports connecting to RopeWar
domain only, but you have the option of connecting using one of the protocols
below:

- TCP (using Protobuf)
- SignalR

### Startup projects

All types of applications depend on Profiles API to get the data of the
character that you are connecting with, so you need to start this project:

- TypingRealm.Profiles.Api

Inter-service communication is done by using locally-issued client
credentials token so we don't overload external Auth0 provider (and since it
only has a limited number of CC tokens, 1000 per month). For local token
generation and validation we need to run this project:

- TypingRealm.IdentityServer.Host

The rest of the projects are dependent on what you want to run.

#### If you plan to run Chat application, start these projects

- TypingRealm.Chat.TcpServer
- TypingRealm.Chat.ConsoleApp (multiple instances)

#### If you plan to run TestClient and use SignalR RopeWar domain, run

- TypingRealm.RopeWar.Server
- TypingRealm.TestClient (multiple instances)

Inside TestClient write "rw" to connect to SignalR RopeWar server.

#### If you plan to run TestClient and use TCP Protobuf RopeWar domain, run

- TypingRealm.RopeWar.TcpServer
- TypingRealm.TestClient (multiple instances)

Inside TestClient write "rwt" to connect to TCP Protobuf RopeWar server.

#### Total list of startup projects

This is the total list of startup projects that I have set up during my
debugging sessions. If you want to just run them all and select what you want
to test already during runtime, you can set them all as startup projects:

- TypingRealm.Profiles.Api
- TypingRealm.IdentityServer.Host
- TypingRealm.Chat.TcpServer
- TypingRealm.Chat.ConsoleApp (multiple instances)
- TypingRealm.RopeWar.Server
- TypingRealm.RopeWar.TcpServer
- TypingRealm.TestClient (multiple instances)

I also have this project as startup project:

- TypingRealm.World.Server

This project is under development and TestClient is not really ready to work
with it yet, but if you are developing part of World domain, you need to enable
it as well.
