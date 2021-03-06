﻿namespace NBB.ProcessManager.Definition
{
    public struct InstanceData<TData>
        where TData : struct
    {
        public object InstanceId { get; }
        public TData Data { get; }

        public InstanceData(object instanceId, TData data)
        {
            InstanceId = instanceId;
            Data = data;
        }
    }
}