# Re.Base

Re.Base is a structured database implementation written entirely in .Net core. As of yet, this is probably only 30% - 35% of an MVP. It is not ready for production use in the slightest

## Inspiration

Currently there are 2 main types of databases to choose from when starting a new project. SQL, and No-SQL. I believe thats a lame set of options. Re.Base is a structured database built on many of the same principles as well-known SQL database implementations. However, it does not use the SQL language. Instead it exposes an API for direct consumption. 

## Goals

Completely open source modular and mod-able database implementation. 

## Usage

The Re.Base API will expose a set of IQueryables based on model structure passed in. I am expecting/hoping for a front end that feels very fluent. The goal should be that it feels as natural to use data from the datasource as if you were using in-memory enumerables. 

`var person = (from p in db.Persons where p.FirstName == "Jon")`

## Contribution

This project is still in early stages. If you would like to contribute, please feel free (however i understand with how early the project is, i am not making that easy). 
