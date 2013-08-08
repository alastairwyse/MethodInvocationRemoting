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

import java.util.*;
import javax.xml.stream.*;
import net.alastairwyse.methodinvocationremoting.*;

/**
 * Overridden version of the MethodInvocationSerializer class, with alternate serialization of booleans, shortened XML tag names, reduced precision when serializing java.lang.Double objects, and support for serializing InterestRateCurve objects.
 * @author Alastair Wyse
 */
public class CustomMethodInvocationSerializer extends MethodInvocationSerializer {

    public CustomMethodInvocationSerializer(ISerializerOperationMap operationMap)  {
        super(operationMap);
        
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
        
        // Override number of digits written when serializing java.lang.Double objects
        doubleFloatingPointDigits = 10;
        
        // Override serialization and deserialization of java.lang.Boolean objects
        operationMap.UpdateMapping(Boolean.class, "bool", new CustomBooleanSerializer());
        
        // Add support for InterestRateCurve class
        operationMap.AddMapping(InterestRateCurve.class, "interestRateCurve", new InterestRateCurveSerializer());
        operationMap.AddMapping(InterestRateCurve[].class, "interestRateCurveArray", genericArraySerializer);
    }
    
    private class CustomBooleanSerializer implements IObjectSerializer<Boolean> {

        @Override
        public void Serialize(Boolean inputBoolean, XMLStreamWriter writer) throws Exception {
            Integer binaryRepresentation;
            if (inputBoolean == true)
            {
                binaryRepresentation = 1;
            }
            else
            {
                binaryRepresentation = 0;
            }
            writer.writeCharacters(binaryRepresentation.toString());
        }

        @Override
        public Boolean Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            Integer binaryRepresentation = Integer.parseInt(reader.ReadString());
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
                throw new DeserializationException("Encountered unexpected number " + binaryRepresentation + " representing a boolean.  Expected either 0 or 1.", binaryRepresentation.toString());
            }
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return booleanValue;
        }
    }
    
    private class InterestRateCurveSerializer implements IObjectSerializer<InterestRateCurve> {

        @Override
        public void Serialize(InterestRateCurve inputInterestRateCurve, XMLStreamWriter writer) throws Exception {
            WriteElementString(writer, "Currency", inputInterestRateCurve.getCurrency());
            writer.writeStartElement("Curve");
            for(Map.Entry<Integer, Double> currentPoint : inputInterestRateCurve.getCurve().entrySet()) {
                writer.writeStartElement("Point");
                WriteElementString(writer, "Term", currentPoint.getKey().toString());
                WriteElementString(writer, "Rate", currentPoint.getValue().toString());
                writer.writeEndElement();
            }
            writer.writeEndElement();
        }

        @Override
        public InterestRateCurve Deserialize(SimplifiedXMLStreamReader reader) throws Exception {
            InterestRateCurve returnInterestRateCurve = new InterestRateCurve();
            returnInterestRateCurve.setCurrency(reader.ReadElementString("Currency"));
            // Consume curve start tag (i.e. <Curve>)
            reader.ReadStartElement("Curve");
            // Attempt to consume point start tag (e.g. <Point>)
            //   If IsNextNodeStartElement() call returns false then the closing </Curve> tag is consumed
            if(reader.IsNextNodeStartElement("Point") == true) {
                // At this point the <Point> tag would have been already read, so need to set base depth 1 level lower than this at <Curve>
                int baseDepth = (reader.getDepth() - 1);
                while(reader.getDepth() >= baseDepth) {
                    Integer currentTerm;
                    Double currentRate;

                    currentTerm = Integer.parseInt(reader.ReadElementString("Term"));
                    currentRate = Double.parseDouble(reader.ReadElementString("Rate"));
                    returnInterestRateCurve.AddPoint(currentTerm, currentRate);
                    reader.ReadEndElement();
                    // Consume parameter start tag (e.g. <Point>)
                    //   If IsNextNodeStartElement() call returns false then the closing </Curve> tag is consumed, baseDepth becomes lower than depth, and the loop will end
                    reader.IsNextNodeStartElement("Point");
                }
            }
            // Consume data end tag (e.g. </Data>)
            reader.ReadEndElement();
            
            return returnInterestRateCurve;
        }
    }
}
