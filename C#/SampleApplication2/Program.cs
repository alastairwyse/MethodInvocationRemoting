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
    /// <summary>
    /// Method Invocation Remoting framework second sample application.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Setup data model, presenter, and view
            ContactListDataModelLayer dataModel = new ContactListDataModelLayer();
            MainViewRemoteAdapter mainView = new MainViewRemoteAdapter("activemq:tcp://localhost:61616?wireFormat.maxInactivityDuration=0", "FromC#", "FromJava", "Request", "Response");
            ContactListPresenter presenter = new ContactListPresenter(mainView, dataModel);
            mainView.SetPresenter(presenter);

            try
            {
                // Connect the view adapter to ActiveMQ and start the application
                mainView.Connect();
                presenter.Start();
                Console.WriteLine("Application running...");
                presenter.ExitRequestedEvent.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                // Disconnect from ActiveMQ
                mainView.Disconnect();
            }
        }
    }
}
