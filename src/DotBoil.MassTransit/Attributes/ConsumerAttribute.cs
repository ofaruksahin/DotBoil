﻿namespace DotBoil.MassTransit.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ConsumerAttribute : Attribute
    {
        public string QueueName { get; set; }

        public ConsumerAttribute(string queueName)
        {
            QueueName = queueName;
        }
    }
}
