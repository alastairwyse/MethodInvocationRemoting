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
using System.Collections;
using System.IO;

namespace MethodInvocationRemoting
{
    //******************************************************************************
    //
    // Class: MethodInvocationSerializerBase
    //
    //******************************************************************************
    /// <summary>
    /// Provides common base methods for classes which implement MethodInvocationRemoting.IMethodInvocationSerializer.
    /// </summary>
    public abstract class MethodInvocationSerializerBase
    {
        //******************************************************************************
        //
        // Method: ConvertStringToMemoryStream
        //
        //******************************************************************************
        /// <summary>
        /// Converts a string to a System.IO.MemoryStream object.
        /// </summary>
        /// <param name="inputString">The string to be converted.</param>
        /// <returns>The string converted to a MemoryStream.</returns>
        protected MemoryStream ConvertStringToMemoryStream(string inputString)
        {
            MemoryStream targetStream = new MemoryStream();
            StreamWriter targetStreamWriter = new StreamWriter(targetStream);

            targetStreamWriter.Write(inputString);
            targetStreamWriter.Flush();
            targetStream.Position = 0;

            return targetStream;
        }

        //******************************************************************************
        //
        // Method: ConvertMemoryStreamToString
        //
        //******************************************************************************
        /// <summary>
        /// Converts the contents of a System.IO.MemoryStream object to a string.
        /// </summary>
        /// <param name="inputMemoryStream">The MemoryStream to be converted.</param>
        /// <returns>The contents of the MemoryStream.</returns>
        /// <remarks>Note that Position property of the MemoryStream will be altered.</remarks>
        protected string ConvertMemoryStreamToString(MemoryStream inputMemoryStream)
        {
            StreamReader sourceStreamReader = new StreamReader(inputMemoryStream);
            inputMemoryStream.Position = 0;
            return sourceStreamReader.ReadToEnd();
        }
    }
}
