![alt text](https://api.travis-ci.com/drewtech/ProcessCompare.svg?branch=master "Build Status")

# ProcessCompare
Execute Red Gate Data Compare with .net core and publish the result to AWS Dynamo DB

This is a .NET core command line program.

It will:

1.  Execute Red Gate Data compare (you need a license for this), and export the results from a saved project.
2.  Take the settings and insert them into AWS Dynamo DB.

Requirements:

DotNet Core 2.2

Red Gate Data Compare

AWS account

