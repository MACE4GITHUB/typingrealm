# TypingRealm

This project is the game where you need to type with your keyboard. The main
project has diverted recently to the direction of first and foremost building a
working typing tool that is able to gather all possible statistics and help you
improve your typing skills, and only later on we might add some gaming modules
to wrap a gameplay around it. With this in mind, everything that is described
below is still valid, but is not the first and foremost concern of this
repository anymore. However, the repository contains a lot of "game" code and
very little "typing" code in comparison (as of now).

The following are the instructions to set up, run and debug (or use, since
it's already fully functional) the typing tool, gathering statistical data that
might help you improve your typing speed.

## Typing tool instructions

These are the instructions on how to run the typing tool (both backend and
frontend).

### Backend services

You need to run the following services:

- TypingRealm.IdentityServer.Host (in theory this is optional, unless you want
to issue & debug tokens, since Profiles API already has an endpoint to get the
test token which is being used by the frontend)
- TypingRealm.Profiles.Api
- TypingRealm.Data.Api

By default (current implementation at the time of writing this README), data is
being stored in *.json files in the folder of TypingRealm.Data.Api project. You
can use the tool to type a lot of texts, and later on you'll be able to analyze
this data since it's being persisted on disk.

### Frontend services

You need to go to "typingrealm-web" folder and run a http server there on port
4200, for example:

```
npm install -g http-server
http-server -p 4200
```

You need to use 4200 port because it's the only port configured in the backend
as allowed by CORS policies. And you need to run the web-server instead of
manually opening `index.html` file cause otherwise CORS policies would fail.

Then you can open your web-browser at the address:

`http://localhost:4200?profile=myName&length=10`

Where
- profile - your profile id, all the data will be associated with this profile.
This way you can have multiple users use the tool.
- length - minimum desired text length, you can set it to 0 if you want to have
default length (which is 10 characters) or you can set it as high as you want.

After you type the whole text, you will see the results of the simulation
(you'll be able to see in real time how you were typing) and the data will have
been saved to the server by now. You can look into the developer console to see
that server sent you an ID of your newly created typing result. If there are no
errors and server returned an ID, this means everything was saved successfully.

To stop the simulation and type the next text, just press Enter.

## TypingRealm Game documentation

Below you can see the "previous" TypingRealm documentation in regards to typing
game first, and only then a typing tool.

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
