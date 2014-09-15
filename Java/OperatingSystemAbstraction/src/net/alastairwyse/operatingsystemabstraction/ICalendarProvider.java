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

package net.alastairwyse.operatingsystemabstraction;

import java.util.Calendar;
import java.util.TimeZone;

/**
 * Defines methods which provide Calendar objects to facilitate mocking and unit testing.
 * The java.util.GregorianCalendar class can only be instantiated using a specific timezone via the constructor.  Hence the class cannot be directly wrapped with an interface to allow mocking.  This interface defines the instantiation of a Calendar class via a method which can be mocked.
 * @author Alastair Wyse
 */
public interface ICalendarProvider {

    /**
     * Returns a Calendar object based on the current time in the given time zone with.
     * @param timeZome  The time zone to use to create the Calendar
     * @return          The Calendar representing the current time in the specified time zone
     */
    public Calendar getCalendar(TimeZone timeZome);
}
