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
using System.Xml;
using MethodInvocationRemoting;

namespace SampleApplication3
{
    /// <summary>
    /// Overridden version of the MethodInvocationRemoting.MethodInvocationSerializer class, with alternate serialization of booleans, shortened XML tag names, reduced precision when serializing System.Double objects, and support for serializing InterestRateCurve objects.
    /// </summary>
    public class CustomMethodInvocationSerializer : MethodInvocationSerializer
    {
        //------------------------------------------------------------------------------
        //
        // Method: CustomMethodInvocationSerializer (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the SampleApplication3.CustomMethodInvocationSerializer class.
        /// </summary>
        /// <param name="operationMap">The serializer operation map to use to store mappings between object types and methods to serialize and deserialize the types.</param>
        public CustomMethodInvocationSerializer(ISerializerOperationMap operationMap) : base(operationMap)
        {
            // Override XML element names
            rootElementName = "MI";
            methodNameElementName = "MN";
            parametersElementName = "Ps";
            parameterElementName = "P";
            dataTypeElementName = "DT";
            dataElementName = "D";
            returnTypeElementName = "RT";
            returnValueElementName = "RV";
            arrayElementDataTypeElementName = "EDT";
            arrayElementElementName = "E";

            // Override number of digits written when serializing System.Double objects
            doubleFloatingPointDigits = 10;

            // Override serialization and deserialization of System.Boolean objects
            operationMap.UpdateMapping(typeof(Boolean), "bool", new XmlSerializationOperation(CustomSerializeBoolean), new XmlDeserializationOperation(CustomDeserializeBoolean));

            // Add support for InterestRateCurve class
            operationMap.AddMapping(typeof(InterestRateCurve), "interestRateCurve", new XmlSerializationOperation(SerializeInterestRateCurve), new XmlDeserializationOperation(DeserializeInterestRateCurve));
            operationMap.AddMapping(typeof(InterestRateCurve[]), "interestRateCurveArray", new XmlSerializationOperation(SerializeArray), new XmlDeserializationOperation(DeserializeArray));
        }

        private void CustomSerializeBoolean(object inputObject, XmlWriter writer)
        {
            Boolean inputBoolean = (Boolean)inputObject;
            int binaryRepresentation;
            if (inputBoolean == true)
            {
                binaryRepresentation = 1;
            }
            else
            {
                binaryRepresentation = 0;
            }
            writer.WriteString(binaryRepresentation.ToString());
        }

        private object CustomDeserializeBoolean(XmlReader reader)
        {
            int binaryRepresentation = Convert.ToInt32(reader.ReadString());
            Boolean booleanValue;
            if (binaryRepresentation == 0)
            {
                booleanValue = false;
            }
            else if (binaryRepresentation == 1)
            {
                booleanValue = true;
            }
            else
            {
                throw new DeserializationException("Encountered unexpected number " + binaryRepresentation + " representing a boolean.  Expected either 0 or 1.", binaryRepresentation.ToString());
            }

            return booleanValue;
        }

        private void SerializeInterestRateCurve(object inputObject, XmlWriter writer)
        {
            InterestRateCurve inputInterestRateCurve = (InterestRateCurve)inputObject;
            writer.WriteElementString("Currency", inputInterestRateCurve.Currency);
            writer.WriteStartElement("Curve");
            foreach (KeyValuePair<int, double> currPoint in inputInterestRateCurve.Curve)
            {
                writer.WriteStartElement("Point");
                writer.WriteElementString("Term", currPoint.Key.ToString());
                writer.WriteElementString("Rate", currPoint.Value.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private object DeserializeInterestRateCurve(XmlReader reader)
        {
            InterestRateCurve returnInterestRateCurve = new InterestRateCurve();

            returnInterestRateCurve.Currency = reader.ReadElementString("Currency");
            if (reader.IsEmptyElement == true)
            {
                // Consume parameters self closing tag (e.g. <Curve />)
                reader.ReadElementString("Curve");
            }
            else
            {
                reader.ReadStartElement("Curve");
                // If the next node is not an end node there are no points in the curve.
                if (IsStartElement("Point", reader) == true)
                {
                    int baseDepth = reader.Depth;

                    while (reader.Depth >= baseDepth)
                    {
                        int currentTerm;
                        double currentRate;

                        reader.ReadStartElement("Point");
                        currentTerm = Convert.ToInt32(reader.ReadElementString("Term"));
                        currentRate = Convert.ToDouble(reader.ReadElementString("Rate"));
                        reader.ReadEndElement();

                        returnInterestRateCurve.Curve.Add(currentTerm, currentRate);
                    }
                }
                reader.ReadEndElement();
            }

            return returnInterestRateCurve;
        }
    }
}
