MethodInvocationRemoting
------------------------

MethodInvocationRemoting is a framework which allows methods to be invoked remotely between C# and Java.

Method invocations are represented as objects and then serialized and transported from code in the calling language (e.g. Java), before being invoked on the receiving side (e.g. C#). Any return values from invoking the method are then passed back and handled in the calling language.

The object model is based around interfaces and an inversion of control / dependency injection design pattern, so users can easily substitute the provided classes which perform serialization and transport with their own classes.

MethodInvocationRemoting includes interchangeable default transport classes which utilize the file system, Apache ActiveMQ message queuing, or a direct TCP network connection, allowing for persistence, performance, or ease of testing depending on the use case. The default serialization classes convert objects to XML, and provide built-in support for many native data types and single-dimension arrays of these types in C# and Java. Additionally the serialization classes can be easily extended to support serialization of user's custom data types.

Whilst remote method invocation is already available between C# and Java using protocols like web services, MethodInvocationRemoting is lightweight and can be utilized in client code without the need for installation and configuration of a separate server-side application, as is the case with web services. MethodInvocationRemoting also provides bidirectionality (duplex networking), so that methods can be invoked from both the C# and Java sides.

Both the C# and Java projects include extensive unit tests to allow for easier extension and refactoring of the code.

For detailed information including explanation of the operation of the classes and sample applications, see http://www.oraclepermissiongenerator.net/methodinvocationremoting/

Detailed logging and instrumentation is available from all major classes in MethodInvocationRemoting using the ApplicationLogging and ApplicationMetrics projects. These two projects are decoupled from MethodInvocationRemoting (interaction is through clearly defined interfaces) and are standalone frameworks in their own right with multiple implementations, which could easily be utilized by other client applications and frameworks.