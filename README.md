# Electron CGI

## Update version 1.0.1 

- Error propagation to Node.js

    - An exception in a handler will be serialized and sent to Node.js (requires electron-cgi 1.0.0) and won't crash the process

- Bugfixes

## Update version 1.0.0

- This version was uploaded incorrectly. Skip it.

## Update version 0.0.5

- Duplex: ability to send requests from both .Net and Node.js

## Update version 0.0.2
- Ability to serve request concurrently (uses System.Threading.Tasks.DataFlow)

### Next steps
- Add the ability to send requests form .Net to Node
- Instead of making the process fail when there's an exception in a handler, serialise the exception and "surface" it in Node
___________

[Electron CGI](https://www.blinkingcaret.com/2019/02/27/electron-cgi/) is a library that enables sending request form NodeJs and have them served by .Net.

The npm package is called [_electron-cgi_](https://www.npmjs.com/package/electron-cgi).

The nuget package is called [ElectronCgi.DotNet](https://www.nuget.org/packages/ElectronCgi.DotNet/#).

Here's an example of how you can interact with a .Net application from Node:

In NodeJs/Electron:

    const { ConnectionBuilder } = require('electron-cgi');

    const connection = new ConnectionBuilder()
            .connectTo('dotnet', 'run', '--project', 'DotNetConsoleProjectWithElectronCgiDotNetNugetPackage')
            .build();

    connection.onDisconnect = () => {
        console.log('Lost connection to the .Net process');
    };
    
    connection.send('greeting', 'John', (err, theGreeting) => {
        if (err) {
            console.log(err);
            return;
        }
        console.log(theGreeting); // will print "Hello John!"
    });

    //or using promises

    const theGreeting = await connection.send('greeting', 'John')

    connection.close();


And in the .Net Console Application:

    using ElectronCgi.DotNet;

    //...
    static void Main(string[] args)
    {
        var connection = new ConnectionBuilder()
                            .WithLogging()
                            .Build();

        // expects a request named "greeting" with a string argument and returns a string
        connection.On<string, string>("greeting", name =>
        {
            return $"Hello {name}!";
        });

        // wait for incoming requests
        connection.Listen();        
    }


### How does it work?

Electron CGI establishes a "connection" with an external process. That external process must be configured to accept that connection. In the example above that's what the `Listen` method does.  

In Node we can "send" requests (for example "greeting" with "John" as a parameter) and receive a response from the other process.

The way this communication channel is established is by using the connected process' stdin and stdout streams. This approach does not rely on staring up a web server and because of that introduces very little overhead in terms of the requests' round-trip time.

