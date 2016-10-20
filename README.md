# DAD2016

Enunciado: https://fenix.tecnico.ulisboa.pt/downloadFile/1689468335567738/DAD-project-description.pdf

## Operator UML:

![Alt text](/Operator/OperatorUML.png?raw=true)
## Issues:

### Generic
> Estrutura de Dados: Tuplos (1 ou + campos (Strings) ordenados)
	

### Operadores
> Communication/interation w/ PCS 
> Notify PuppetMaster (event notifications)
> Operações	
	* UNIQ
	* COUNT
	* DUP
	* FILTER
	* CUSTOM
> Ler ficheiros
> Receber dados produzidos por operadores 
> Escrever ficheiros 
> Tuple Routing : Primary, Random, Hashing


### PCS (Process Creation Service)
> Criação do Servico - Replication Service (Porto 10000)


### PuppetMaster
> Parse Config_file : Includes relevant process initiation (to be done after parse config file...) ??
> Ler Script_file c/ comandos.
> Interface (Command Line - Ler) 
> Interface (Command Line - Escrever) 
> Interface : Logging
> Communication/integration w/ PCS 
> Operações Externas (necessário implemntar lógica nos operadores): Start, Interval, Status, Crash, Freeze, Unfreeze
> Operação : Wait (disponivel apenas para o script file)


### Fault Tolerance

> Semanticas de Execução: At Most Once
> Semanticas de Execução: At least Once
> Semanticas de Execução: Exaclty Once

> Crash fail - Redirect output automaticly / transparently 
