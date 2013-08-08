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

package net.alastairwyse.methodinvocationremoting;

/**
 * Container for properties of a method and the parameters used when the method is called or invoked.
 * @author Alastair Wyse
 */
public interface IMethodInvocation {

    /**
     * @return  The name of the method.
     */
    public String getName();
    
    /**
     * @return  The parameters sent when the method is invoked.
     */
    public Object[] getParameters();
    
    /**
     * @return  The return type of the method.
     */
    public Class<?> getReturnType();
}
