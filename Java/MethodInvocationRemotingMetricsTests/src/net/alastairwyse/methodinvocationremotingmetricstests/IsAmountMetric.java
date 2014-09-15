/*
 * Copyright 2014 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

package net.alastairwyse.methodinvocationremotingmetricstests;

import net.alastairwyse.applicationmetrics.AmountMetric;

import org.hamcrest.Description;
import org.mockito.ArgumentMatcher;

/**
 * Extension of the mockito ArgumentMatcher class, which can be used in mockito verify() and when() methods to confirm that a parameter passed in test code matches the type and amount of the AmountMetric specified in the constructor. 
 * @author Alastair Wyse
 */
public class IsAmountMetric extends ArgumentMatcher<AmountMetric> {

    private AmountMetric amountMetric;
    
    public IsAmountMetric(AmountMetric amountMetric) {
        this.amountMetric = amountMetric;
    }
    
    @Override
    public boolean matches(Object argument) {
        if (argument.getClass() != amountMetric.getClass()) {
            return false;
        }
        
        AmountMetric comparisonAmountMetric = (AmountMetric)argument;
        if (amountMetric.getAmount() != comparisonAmountMetric.getAmount()) {
            return false;
        }

        return true;
    }
    
    @Override
    public void describeTo(Description description) {
        description.appendText(amountMetric.getClass().getSimpleName() + "(" + amountMetric.getAmount() + ")");
    }
}
