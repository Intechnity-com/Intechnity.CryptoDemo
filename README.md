# README: Intechnity.CryptoDemo

## Introduction

Intechnity.CryptoDemo is a basic blockchain implementation developed in C#. The main aim of this project is to experiment with blockchain technology, explore its core principles, and understand how it works in practice.
This application is not intended for production usage, but rather for learning and educational purposes.

## Configuration

The configuration for the blockchain and the Kestrel server is defined in the `appsettings.json` file.

- `BlockchainConfiguration` contains information regarding the blockchain ID and the Known IP addresses, which are the IP addresses of the other nodes in the blockchain network. 
- `Kestrel` is the cross-platform web server for ASP.NET Core applications. Here, you can define the HTTP end points for the application.

## Getting Started

To run the project, first, clone the repository to your local machine.

```shell
git clone https://github.com/Intechnity-com/Intechnity.CryptoDemo.git
```

Navigate into the project directory:

```shell
cd Intechnity.CryptoDemo\src\cryptodemo\Intechnity.CryptoDemo.Console
```

You can then use .NET Core CLI to run the project:

```shell
dotnet run
```

This will start the application on the default port as specified in the appsettings.json file.

## Starting Multiple Instances
To simulate a blockchain network and test the interaction between multiple nodes, you can start multiple instances of the application on different ports. This can be done by overriding the Kestrel:EndPoints:Http:Url and BlockchainConfiguration:KnownIpAddresses settings in the command line as follows:

```shell
dotnet run --Kestrel:Endpoints:Http:Url="http://*:60113"
dotnet run --Kestrel:Endpoints:Http:Url="http://*:60114" --BlockchainConfiguration:KnownIpAddresses:0="http://localhost:60113"
dotnet run --Kestrel:Endpoints:Http:Url="http://*:60115" --BlockchainConfiguration:KnownIpAddresses:0="http://localhost:60113"
```

Each of these commands will start a new instance of the application on the specified port and override the KnownIpAddresses in the BlockchainConfiguration so that each instance recognizes other nodes in the network.