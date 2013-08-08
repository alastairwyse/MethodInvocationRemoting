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
 * Defines the methods of the main view in the contact list application MVP model.
 * @author Alastair Wyse
 */
public interface IMainView {

    /**
     * Sets the presenter which this view should interact with.
     * @param presenter  The presenter.
     */
    void SetPresenter(IContactListPresenter presenter);

    /**
     * Clears the contents of all elements in the view.
     */
    void Initialise() throws Exception;
    
    /**
     * Displays the view.
     */
    void Show() throws Exception;

    /**
     * Closes the view.
     */
    void Close();

    /**
     * Adds or updates a contact in the main grid in the view, using the contact name as a key to decide whether to insert a new contact or update an existing.
     * @param name          The name of the contact.
     * @param category      The category of the contact.
     * @param phoneNumber   The phone number of the contact.
     * @param emailAddress  The email address of the contact.
     */
    void AddUpdateContactInGrid(String name, String category, String phoneNumber, String emailAddress) throws Exception;

    /**
     * Deletes a contact from the a main grid in the view.
     * @param name  The name of the contact to delete.
     */
    void DeleteContactFromGrid(String name) throws Exception;

    /**
     * Populates the category drop down in the view.
     * @param categories  The valid categories.
     */
    void PopulateCategories(String[] categories) throws Exception;

    /**
     * Displays an error message.
     * @param errorMessage  The error message.
     */
    void DisplayErrorMessage(String errorMessage) throws Exception;
}
