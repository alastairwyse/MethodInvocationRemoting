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

namespace SampleApplication2
{
    //******************************************************************************
    //
    // Interface: IContactListDataModelLayer
    //
    //******************************************************************************
    /// <summary>
    /// Defines the methods of the data model layer in the contact list application MVP model
    /// </summary>
    public interface IContactListDataModelLayer
    {
        /// <summary>
        /// Adds or updates a contact in the data model, using the contact name as a key to decide whether to insert a new contact or update an existing.
        /// </summary>
        /// <param name="name">The name of the contact.</param>
        /// <param name="category">The category of the contact.</param>
        /// <param name="phoneNumber">The phone number of the contact.</param>
        /// <param name="emailAddress">The email address of the contact.</param>
        void AddUpdateContact(String name, String category, String phoneNumber, String emailAddress);

        /// <summary>
        /// Deletes a contact from the data model.
        /// </summary>
        /// <param name="name">The name of the contact person.</param>
        void DeleteContact(String name);

        /// <summary>
        /// Retrieves all valid contact categories in the data model.
        /// </summary>
        /// <returns>The valid contact categories.</returns>
        List<String> GetCategories();

        /// <summary>
        /// Checks whether the inputted name is valid in the data model.
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if the name is valid, otherwise false.</returns>
        bool NameIsValid(string name);

        /// <summary>
        /// Checks whether the inputted email address is valid in the data model.
        /// </summary>
        /// <param name="emailAddress">The email address to check.</param>
        /// <returns>True if the email address is valid, otherwise false.</returns>
        bool EmailAddressIsValid(string emailAddress);
    }
}
