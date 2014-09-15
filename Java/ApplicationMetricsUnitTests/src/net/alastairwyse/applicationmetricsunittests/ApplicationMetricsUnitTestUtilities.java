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

package net.alastairwyse.applicationmetricsunittests;

import java.util.*;

/**
 * Common utility methods to support unit tests of the net.alastairwyse.applicationmetrics package.
 * @author Alastair Wyse
 */
class ApplicationMetricsUnitTestUtilities {

    /**
     * Initialises a new instance of the ApplicationMetricsUnitTestUtilities class.  
     */
    public ApplicationMetricsUnitTestUtilities() {
        
    }
    
    /**
     * Creates and returns a GregorianCalendar using the specified time including milliseconds.
     * @param   year                The year.
     * @param   month               The month.
     * @param   day                 The day.
     * @param   hour                The hours.
     * @param   minute              The minutes.
     * @param   second              The seconds.
     * @param   milliseconds        The milliseconds.
     * @return  The GregorianCalendar.
     */
    public GregorianCalendar CreateCalendarWithMilliseconds(int year, int month, int day, int hour, int minute, int second, int milliseconds) {
        GregorianCalendar returnCalendar = new GregorianCalendar(year, month, day, hour, minute, second);
        // Need to call a get method on the calendar or the time zone returned can be unpredictable due to bug in java...
        //   http://bugs.java.com/bugdatabase/view_bug.do?bug_id=4827490
        returnCalendar.get(GregorianCalendar.HOUR_OF_DAY);
        returnCalendar.add(Calendar.MILLISECOND, milliseconds);
        return returnCalendar;
    }
}
