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

using System;
using System.Collections.Generic;
using System.Text;

namespace OperatingSystemAbstraction
{
    //******************************************************************************
    //
    // Class: PerformanceCounter
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:OperatingSystemAbstraction.IPerformanceCounter"]/*'/>
    class PerformanceCounter : IPerformanceCounter
    {
        protected bool disposed;
        private System.Diagnostics.PerformanceCounter performanceCounter;

        //------------------------------------------------------------------------------
        //
        // Method: PerformanceCounter (constructor)
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Initialises a new instance of the OperatingSystemAbstraction.PerformanceCounter class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="readOnly">true to access the counter in read-only mode (although the counter itself could be read/write); false to access the counter in read/write mode.</param>
        public PerformanceCounter(string categoryName, string counterName, bool readOnly)
        {
            performanceCounter = new System.Diagnostics.PerformanceCounter(categoryName, counterName, readOnly);
        }

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="P:OperatingSystemAbstraction.IPerformanceCounter.RawValue"]/*'/>
        public long RawValue
        {
            set
            {
                performanceCounter.RawValue = value;
            }
        }

        #region Finalize / Dispose Methods

        /// <summary>
        /// Releases the unmanaged resources used by the PerformanceCounter.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PerformanceCounter()
        {
            Dispose(false);
        }

        //------------------------------------------------------------------------------
        //
        // Method: Dispose
        //
        //------------------------------------------------------------------------------
        /// <summary>
        /// Provides a method to free unmanaged resources used by this class.
        /// </summary>
        /// <param name="disposing">Whether the method is being called as part of an explicit Dispose routine, and hence whether managed resources should also be freed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                if (performanceCounter != null)
                {
                    performanceCounter.Dispose();
                }
                // Set large fields to null.
                performanceCounter = null;

                disposed = true;
            }
        }

        #endregion
    }
}
