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
using System.Threading;

namespace SampleApplication2
{
    //******************************************************************************
    //
    // Class: ContactListPresenter
    //
    //******************************************************************************
    /// <summary>
    /// The presenter layer in the contact list application MVP model.
    /// </summary>
    public class ContactListPresenter : IContactListPresenter
    {
        private IContactListDataModelLayer dataModel;
        private IMainView mainView;
        private AutoResetEvent exitRequestedEvent;

        /// <summary>
        /// AutoResetEvent which is set when a request has been made to exit the application.
        /// </summary>
        public AutoResetEvent ExitRequestedEvent
        {
            get
            {
                return exitRequestedEvent;
            }
        }

        //******************************************************************************
        //
        // Method: ContactListPresenter (constructor)
        //
        //******************************************************************************
        public ContactListPresenter(IMainView mainView, IContactListDataModelLayer dataModel)
        {
            exitRequestedEvent = new AutoResetEvent(false);
            this.mainView = mainView;
            this.dataModel = dataModel;
        }

        public void Start()
        {
            List<String> categories = dataModel.GetCategories();
            String[] catgoriesArray = categories.ToArray();
            mainView.Initialise();
            mainView.PopulateCategories(catgoriesArray);
            mainView.Show();
        }

        public void AddUpdateContact(string name, string category, string phoneNumber, string emailAddress)
        {
            if (dataModel.NameIsValid(name) == false)
            {
                mainView.DisplayErrorMessage("The name is invalid");
                return;
            }
            if (dataModel.EmailAddressIsValid(emailAddress) == false)
            {
                mainView.DisplayErrorMessage("The email address is invalid");
                return;
            }
            dataModel.AddUpdateContact(name, category, phoneNumber, emailAddress);
            mainView.AddUpdateContactInGrid(name, category, phoneNumber, emailAddress);
        }

        public void DeleteContact(string name)
        {
            dataModel.DeleteContact(name);
            mainView.DeleteContactFromGrid(name);
        }

        public void Exit()
        {
            mainView.Close();
            exitRequestedEvent.Set();
        }
    }
}
