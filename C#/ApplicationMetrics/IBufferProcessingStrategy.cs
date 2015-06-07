/*
 * Copyright 2015 Alastair Wyse (http://www.oraclepermissiongenerator.net/methodinvocationremoting/)
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

namespace ApplicationMetrics
{
    //******************************************************************************
    //
    // Interface: IBufferProcessingStrategy
    //
    //******************************************************************************
    /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="T:ApplicationMetrics.IBufferProcessingStrategy"]/*'/>
    public interface IBufferProcessingStrategy
    {
        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="E:ApplicationMetrics.IBufferProcessingStrategy.BufferProcessed"]/*'/>
        event EventHandler BufferProcessed;

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Start"]/*'/>
        void Start();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Stop"]/*'/>
        void Stop();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.Stop(System.Boolean)"]/*'/>
        void Stop(bool processRemainingBufferedMetricEvents);

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyCountMetricEventBuffered"]/*'/>
        void NotifyCountMetricEventBuffered();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyAmountMetricEventBuffered"]/*'/>
        void NotifyAmountMetricEventBuffered();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyStatusMetricEventBuffered"]/*'/>
        void NotifyStatusMetricEventBuffered();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyIntervalMetricEventBuffered"]/*'/>
        void NotifyIntervalMetricEventBuffered();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyCountMetricEventBufferCleared"]/*'/>
        void NotifyCountMetricEventBufferCleared();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyAmountMetricEventBufferCleared"]/*'/>
        void NotifyAmountMetricEventBufferCleared();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyStatusMetricEventBufferCleared"]/*'/>
        void NotifyStatusMetricEventBufferCleared();

        /// <include file='InterfaceDocumentationComments.xml' path='doc/members/member[@name="M:ApplicationMetrics.IBufferProcessingStrategy.NotifyIntervalMetricEventBufferCleared"]/*'/>
        void NotifyIntervalMetricEventBufferCleared();
    }
}
