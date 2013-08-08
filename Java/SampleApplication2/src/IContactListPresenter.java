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

/**
 * Defines the methods of the presenter layer in the contact list application MVP model.
 * @author Alastair Wyse
 */
public interface IContactListPresenter {

    /**
     * Adds or updates a contact in the data model layer.
     * @param name          The name of the contact.
     * @param category      The category of the contact.
     * @param phoneNumber   The phone number of the contact.
     * @param emailAddress  The email address of the contact.
     */
    void AddUpdateContact(String name, String category, String phoneNumber, String emailAddress);

    /**
     * Deletes a contact from the data model layer.
     * @param name  The name of the contact person.
     */
    void DeleteContact(String name);

    /**
     * Exits the application.
     */
    void Exit();
}
