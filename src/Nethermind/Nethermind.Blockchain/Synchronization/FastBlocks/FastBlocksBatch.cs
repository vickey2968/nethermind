/*
 * Copyright (c) 2018 Demerzel Solutions Limited
 * This file is part of the Nethermind library.
 *
 * The Nethermind library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * The Nethermind library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics;

namespace Nethermind.Blockchain.Synchronization.FastBlocks
{
    public class FastBlocksBatch
    {
        private Stopwatch _stopwatch = new Stopwatch();
        private long? ScheduledLastTime;
        private long? RequestSentTime;
        private long? ValidationStartTime;
        private long? HandlingStartTime;
        private long? HandlingEndTime;
        
        public bool Prioritized { get; set; }

        public FastBlocksBatchType BatchType => Headers != null ? FastBlocksBatchType.Headers : Bodies == null ? FastBlocksBatchType.Bodies : FastBlocksBatchType.Receipts;
        public ReceiptsSyncBatch Receipts { get; set; }
        public HeadersSyncBatch Headers { get; set; }
        public BodiesSyncBatch Bodies { get; set; }
        public SyncPeerAllocation Allocation { get; set; }
        public PeerInfo PreviousPeerInfo { get; set; }

        public FastBlocksBatch()
        {
            _stopwatch.Start();
            ScheduledLastTime = _stopwatch.ElapsedMilliseconds;
        }
        
        public void MarkRetry()
        {
            Retries++;
            ScheduledLastTime = _stopwatch.ElapsedMilliseconds;
            ValidationStartTime = null;
            RequestSentTime = null;
            HandlingStartTime = null;
            HandlingEndTime = null;
        }
        
        public void MarkSent()
        {
            RequestSentTime = _stopwatch.ElapsedMilliseconds;
            
        }
        public void MarkValidation()
        {
            ValidationStartTime = _stopwatch.ElapsedMilliseconds;
        }
        
        public void MarkHandlingStart()
        {
            HandlingStartTime = _stopwatch.ElapsedMilliseconds;
        }
        
        public void MarkHandlingEnd()
        {
            HandlingEndTime = _stopwatch.ElapsedMilliseconds;
        }
        
        private int Retries { get; set; }
        public double? AgeInMs => _stopwatch.ElapsedMilliseconds;
        public double? SchedulingTime => (RequestSentTime ?? _stopwatch.ElapsedMilliseconds) - (ScheduledLastTime ?? _stopwatch.ElapsedMilliseconds);
        public double? RequestTime => (ValidationStartTime ?? _stopwatch.ElapsedMilliseconds) - (RequestSentTime ?? _stopwatch.ElapsedMilliseconds);
        public double? ValidationTime => (HandlingStartTime ?? _stopwatch.ElapsedMilliseconds) - (ValidationStartTime ?? HandlingStartTime ?? _stopwatch.ElapsedMilliseconds);
        public double? HandlingTime => (HandlingEndTime ?? _stopwatch.ElapsedMilliseconds) - (HandlingStartTime ?? _stopwatch.ElapsedMilliseconds);
        public long? MinNumber { get; set; }
        
        public override string ToString()
        {
            string startBlock = Headers?.StartNumber.ToString();
            string endBlock = (Headers?.StartNumber != null ? Headers.StartNumber + (Headers.RequestSize - 1) : (Headers?.RequestSize ?? 0) - 1).ToString();
            string details = string.Empty;
            switch (BatchType)
            {
                case FastBlocksBatchType.None:
                    break;
                case FastBlocksBatchType.Headers:
                    details = $"[{startBlock}, {endBlock}]({Headers?.RequestSize ?? Bodies?.Request.Length})";
                    break;
                case FastBlocksBatchType.Bodies:
                    details = $"({Bodies.Request.Length})";
                    break;
                case FastBlocksBatchType.Receipts:
                    details = $"({Receipts.BlockHashes.Length})";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            string priority = Prioritized ? "HIGH" : "LOW";
            return $"{BatchType} {details} [{priority}] [times: S:{SchedulingTime:F0}ms|R:{RequestTime:F0}ms|V:{ValidationTime:F0}ms|H:{HandlingTime:F0}ms|A:{AgeInMs:F0}ms, retries {Retries}] min#: {MinNumber} {Allocation?.Current ?? PreviousPeerInfo}";
        }
    }
}