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
    // Interface: IMainView
    //
    //******************************************************************************
    /// <summary>
    /// Defines the methods of the main view in the contact list application MVP model.
    /// </summary>
    public interface IMainView
    {
        /// <summary>
        /// Sets the presenter which this view should interact with.
        /// </summary>
        /// <param name="presenter">The presenter.</param>
        void SetPresenter(IContactListPresenter presenter);

        /// <summary>
        /// Clears the contents of all elements in the view.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Displays the view.
        /// </summary>
        void Show();

        /// <summary>
        /// Closes the view.
        /// </summary>
        void Close();

        /// <summary>
        /// Adds or updates a contact in the main grid in the view, using the contact name as a key to decide whether to insert a new contact or update an existing.
        /// </summary>
        /// <param name="name">The name of the contact.</param>
        /// <param name="category">The category of the contact.</param>
        /// <param name="phoneNumber">The phone number of the contact.</param>
        /// <param name="emailAddress">The email address of the contact.</param>
        void AddUpdateContactInGrid(String name, String category, String phoneNumber, String emailAddress);

        /// <summary>
        /// Deletes a contact from the a main grid in the view.
        /// </summary>
        /// <param name="name">The name of the contact to delete.</param>
        void DeleteContactFromGrid(String name);

        /// <summary>
        /// Populates the category drop down in the view.
        /// </summary>
        /// <param name="categories">The valid categories.</param>
        void PopulateCategories(String[] categories);

        /// <summary>
        /// Displays an error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        void DisplayErrorMessage(String errorMessage);
    }
}
