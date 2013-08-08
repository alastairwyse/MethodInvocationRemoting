/*
 * Copyright 2013 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using MethodInvocationRemoting;

namespace SampleApplication3
{
    /// <summary>
    /// Method Invocation Remoting framework third sample application.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // ------------------
            // Test Serialization
            // ------------------
            CustomMethodInvocationSerializer customSerializer = new CustomMethodInvocationSerializer(new SerializerOperationMap());

            // Create parameters of the method invocation
            InterestRateCurve rateCurveParameter = new InterestRateCurve();
            rateCurveParameter.Currency = "AUD";
            rateCurveParameter.AddPoint(24, 2.56);
            rateCurveParameter.AddPoint(60, 2.85);
            rateCurveParameter.AddPoint(120, 3.45);
            rateCurveParameter.AddPoint(180, 3.82);

            InterestRateCurve[] rateCurveArrayParameter = new InterestRateCurve[3];
            rateCurveArrayParameter[0] = new InterestRateCurve();
            rateCurveArrayParameter[0].Currency = "GBP";
            rateCurveArrayParameter[0].AddPoint(24, 0.75);
            rateCurveArrayParameter[0].AddPoint(60, 1.37);
            rateCurveArrayParameter[0].AddPoint(120, 2.31);
            rateCurveArrayParameter[0].AddPoint(180, 2.86);
            rateCurveArrayParameter[1] = new InterestRateCurve();
            rateCurveArrayParameter[1].Currency = "JPY";
            rateCurveArrayParameter[1].AddPoint(24, 0.18583);
            rateCurveArrayParameter[1].AddPoint(60, 0.38375);
            rateCurveArrayParameter[1].AddPoint(120, 0.83771);
            rateCurveArrayParameter[1].AddPoint(180, 1.65875);
            rateCurveArrayParameter[2] = new InterestRateCurve();
            rateCurveArrayParameter[2].Currency = "USD";
            rateCurveArrayParameter[2].AddPoint(24, 0.35);
            rateCurveArrayParameter[2].AddPoint(60, 0.8);
            rateCurveArrayParameter[2].AddPoint(120, 1.93);
            rateCurveArrayParameter[2].AddPoint(180, 2.36);

            Boolean booleanParameter = true;

            Boolean[] booleanArrayParameter = new Boolean[3];
            booleanArrayParameter[0] = false;
            booleanArrayParameter[1] = true;
            booleanArrayParameter[2] = false;

            Double doubleParameter = 3.14159265358979323846264338327950288419716939937510582;

            object[] parameters = new object[5];
            parameters[0] = rateCurveParameter;
            parameters[1] = rateCurveArrayParameter;
            parameters[2] = booleanParameter;
            parameters[3] = booleanArrayParameter;
            parameters[4] = doubleParameter;

            // Create and serialize the method invocation
            MethodInvocation testMethodInvocation = new MethodInvocation("TestMethodInvocation", parameters);
            Console.WriteLine(customSerializer.Serialize(testMethodInvocation));
            Console.WriteLine();

            // --------------------
            // Test Deserialization
            // --------------------
            String serializedMethodInvocation = "<?xml version=\"1.0\" encoding=\"utf-8\"?><MI><MN>TestMethodInvocation</MN><Ps><P><DT>interestRateCurve</DT><D><Currency>AUD</Currency><Curve><Point><Term>24</Term><Rate>2.56</Rate></Point><Point><Term>180</Term><Rate>3.82</Rate></Point><Point><Term>120</Term><Rate>3.45</Rate></Point><Point><Term>60</Term><Rate>2.85</Rate></Point></Curve></D></P><P><DT>interestRateCurveArray</DT><D><EDT>interestRateCurve</EDT><E><DT>interestRateCurve</DT><D><Currency>GBP</Currency><Curve><Point><Term>24</Term><Rate>0.75</Rate></Point><Point><Term>180</Term><Rate>2.86</Rate></Point><Point><Term>120</Term><Rate>2.31</Rate></Point><Point><Term>60</Term><Rate>1.37</Rate></Point></Curve></D></E><E><DT>interestRateCurve</DT><D><Currency>JPY</Currency><Curve><Point><Term>24</Term><Rate>0.18583</Rate></Point><Point><Term>180</Term><Rate>1.65875</Rate></Point><Point><Term>120</Term><Rate>0.83771</Rate></Point><Point><Term>60</Term><Rate>0.38375</Rate></Point></Curve></D></E><E><DT>interestRateCurve</DT><D><Currency>USD</Currency><Curve><Point><Term>24</Term><Rate>0.35</Rate></Point><Point><Term>180</Term><Rate>2.36</Rate></Point><Point><Term>120</Term><Rate>1.93</Rate></Point><Point><Term>60</Term><Rate>0.8</Rate></Point></Curve></D></E></D></P><P><DT>bool</DT><D>1</D></P><P><DT>boolArray</DT><D><EDT>bool</EDT><E><DT>bool</DT><D>0</D></E><E><DT>bool</DT><D>1</D></E><E><DT>bool</DT><D>0</D></E></D></P><P><DT>double</DT><D>3.1415926536E0</D></P></Ps><RT></RT></MI>";

            // Deserialize the method invocation
            testMethodInvocation = customSerializer.Deserialize(serializedMethodInvocation);

            // Extract the parameters
            rateCurveParameter = (InterestRateCurve)testMethodInvocation.Parameters[0];
            rateCurveArrayParameter = (InterestRateCurve[])testMethodInvocation.Parameters[1];
            booleanParameter = (Boolean)testMethodInvocation.Parameters[2];
            booleanArrayParameter = (Boolean[])testMethodInvocation.Parameters[3];
            doubleParameter = (Double)testMethodInvocation.Parameters[4];
            
            // Print the contents of the parameters
            Console.WriteLine("-- Interest Rate Curve Parameter --");
            PrintInterestRateCurve(rateCurveParameter);
            Console.WriteLine();
            
            Console.WriteLine("-- Interest Rate Curve Array Parameter --");
            PrintInterestRateCurve(rateCurveArrayParameter[0]);
            PrintInterestRateCurve(rateCurveArrayParameter[1]);
            PrintInterestRateCurve(rateCurveArrayParameter[2]);
            Console.WriteLine();
            
            Console.WriteLine("-- Boolean Parameter --");
            Console.WriteLine("Value: " + booleanParameter.ToString());
            Console.WriteLine();

            Console.WriteLine("-- Boolean Array Parameter --");
            Console.WriteLine("Element 1 value: " + booleanArrayParameter[0].ToString());
            Console.WriteLine("Element 2 value: " + booleanArrayParameter[1].ToString());
            Console.WriteLine("Element 3 value: " + booleanArrayParameter[2].ToString());
            Console.WriteLine();
            
            Console.WriteLine("-- Double Parameter --");
            Console.WriteLine("Value: " + doubleParameter.ToString());
            Console.WriteLine();
        }

        static void PrintInterestRateCurve(InterestRateCurve inputCurve)
        {
            Console.WriteLine("Currency: " + inputCurve.Currency);
            foreach (KeyValuePair<int, double> currentPoint in inputCurve.Curve)
            {
                Console.WriteLine("  " + currentPoint.Key + "\t" + currentPoint.Value);
            }
        }
    }
}
