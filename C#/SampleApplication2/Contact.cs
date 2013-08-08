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
    // Class: Contact
    //
    //******************************************************************************
    /// <summary>
    /// Container class representing a contact person, and including the person's contact information.
    /// </summary>
    class Contact
    {
        private String name;
        private String category;
        private String phoneNumber;
        private String emailAddress;

        /// <summary>
        /// The name of the contact person.
        /// </summary>
        public String Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// The category of the contact person.
        /// </summary>
        public String Category
        {
            get
            {
                return category;
            }
            set
            {
                category = value;
            }
        }

        /// <summary>
        /// The phone number of the contact person.
        /// </summary>
        public String PhoneNumber
        {
            get
            {
                return phoneNumber;
            }
            set
            {
                phoneNumber = value;
            }
        }

        /// <summary>
        /// The email address of the contact person.
        /// </summary>
        public String EmailAddress
        {
            get
            {
                return emailAddress;
            }
            set
            {
                if (EmailAddressIsValid(value) == true)
                {
                    emailAddress = value;
                }
                else
                {
                    throw new ArgumentException("The email address is invalid.", "EmailAddress");
                }
            }
        }

        //******************************************************************************
        //
        // Method: Contact (constructor)
        //
        //******************************************************************************
        /// <summary>
        /// Initialises a new instance of the SampleApplication2.Contact class.
        /// </summary>
        /// <param name="name">The name of the contact person.</param>
        /// <param name="category">The category of the contact person.</param>
        /// <param name="phoneNumber">The phone number of the contact person.</param>
        /// <param name="emailAddress">The email address of the contact person.</param>
        public Contact(String name, String category, String phoneNumber, String emailAddress)
        {
            if (name.Trim() == "")
            {
                throw new ArgumentException("The name argument must contain printable characters.", "name");
            }
            this.name = name;
            this.category = category;
            this.phoneNumber = phoneNumber;
            this.EmailAddress = emailAddress;
        }

        //******************************************************************************
        //
        // Method: NameIsValid
        //
        //******************************************************************************
        /// <summary>
        /// Assesses whether the inputted name is valid to set on the class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Returns true if the name is valid.</returns>
        public static bool NameIsValid(string name)
        {
            if (name.Trim() == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //******************************************************************************
        //
        // Method: EmailAddressIsValid
        //
        //******************************************************************************
        /// <summary>
        /// Assesses whether the inputted email address is valid to set on the class.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns>Returns true if the email address is valid.</returns>
        public static bool EmailAddressIsValid(string emailAddress)
        {
            int atSymbolInstances = 0;

            foreach (char currentChar in emailAddress)
            {
                if (currentChar == '@')
                {
                    atSymbolInstances = atSymbolInstances + 1;
                }
            }

            if (atSymbolInstances == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
