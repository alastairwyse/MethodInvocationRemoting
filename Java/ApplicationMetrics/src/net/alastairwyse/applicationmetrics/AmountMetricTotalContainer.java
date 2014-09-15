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

package net.alastairwyse.applicationmetrics;

/**
 * Container class which stores an amount metric, and the total amount of all instances of the metric.
 * @author Alastair Wyse
 */
class AmountMetricTotalContainer {

    private AmountMetric amountMetric;
    private long total;

    /**
     * @return  The amount metric for which the total is stored.
     */
    public AmountMetric getAmountMetric() {
        return amountMetric;
    }

    /**
     * @return  The total amount of all instances of the metric.
     */
    public long getTotal() {
        return total;
    }

    /**
     * Initialises a new instance of the AmountMetricTotalContainer class.
     * @param amountMetric  The amount metric for which the total stored.
     */
    public AmountMetricTotalContainer(AmountMetric amountMetric){
        this.amountMetric = amountMetric;
        total = 0;
    }

    /**
     * Adds the specified amount to the stored total.
     * @param amount  The amount to add.
     */
    public void Add(long amount){
        if ((Long.MAX_VALUE - total) >= amount) {
            total = total + amount;
        }
    }
}
