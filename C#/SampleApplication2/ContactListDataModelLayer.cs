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
    // Class: ContactListDataModelLayer
    //
    //******************************************************************************
    /// <summary>
    /// The data model layer in the contact list application MVP model
    /// </summary>
    public class ContactListDataModelLayer : IContactListDataModelLayer
    {
        private List<Contact> contactList;
        private List<String> validCategories;

        //******************************************************************************
        //
        // Method: ContactListDataModelLayer (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the SampleApplication2.ContactListDataModelLayer class.
        /// </summary>
        public ContactListDataModelLayer()
        {
            contactList = new List<Contact>();
            validCategories = new List<string>();
            validCategories.Add("Friend");
            validCategories.Add("Family");
            validCategories.Add("Work");
        }

        public void AddUpdateContact(string name, string category, string phoneNumber, string emailAddress)
        {
            if (validCategories.Contains(category) == false)
            {
                throw new ArgumentException("Contact category '" + category + "' is invalid.", "category");
            }

            int updateIndex = -1;
            for (int i = 0; i < contactList.Count; i = i + 1)
            {
                if (contactList[i].Name == name)
                {
                    updateIndex = i;
                    break;
                }
            }

            if (updateIndex == -1)
            {
                contactList.Add(new Contact(name, category, phoneNumber, emailAddress));
            }
            else
            {
                contactList[updateIndex] = new Contact(name, category, phoneNumber, emailAddress);
            }
        }

        public void DeleteContact(string name)
        {
            int deleteIndex = -1;

            for (int i = 0; i < contactList.Count; i = i + 1)
            {
                if (contactList[i].Name == name)
                {
                    deleteIndex = i;
                    break;
                }
            }

            if (deleteIndex == -1)
            {
                throw new ArgumentException("Contact with name '" + name + "' does not exist.", "name");
            }
            else
            {
                contactList.RemoveAt(deleteIndex);
            }
        }

        public List<String> GetCategories()
        {
            return validCategories;
        }

        public bool NameIsValid(string name)
        {
            return Contact.NameIsValid(name);
        }

        public bool EmailAddressIsValid(string emailAddress)
        {
            return Contact.EmailAddressIsValid(emailAddress);
        }
    }
}
