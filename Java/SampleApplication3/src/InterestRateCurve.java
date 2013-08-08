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

/**
 * Represents an interest rate curve.
 * @author Alastair Wyse
 */
public class InterestRateCurve {

    private String currency;
    private HashMap<Integer, Double> curve;
    
    /**
     * Initialises a new instance of the InterestRateCurve class.
     */
    public InterestRateCurve()
    {
        curve = new HashMap<Integer, Double>();
    }
    
    /**
     * Sets the currency of the interest rate curve.
     * @param currency  The currency.
     */
    public void setCurrency(String currency) {
        this.currency = currency;
    }
    
    /**
     * Returns the currency of the interest rate curve.
     * @return  The currency.
     */
    public String getCurrency() {
        return currency;
    }
    
    /**
     * Returns the interest rate curve.
     * @return  The interest rate curve.
     */
    public HashMap<Integer, Double> getCurve() {
        return curve;
    }
    
    /**
     * Adds a data point to the curve.
     * @param term  The term or time dimension of the data point in months.
     * @param rate  The interest rate of the data point.
     */
    public void AddPoint(Integer term, Double rate) {
        curve.put(term, rate);
    }
}
