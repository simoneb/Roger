﻿using System.Collections.Generic;
using RabbitMQ.Client;

namespace Roger
{
    public interface IMessageFilter
    {
        IEnumerable<CurrentMessageInformation> Filter(IEnumerable<CurrentMessageInformation> input, IModel model);
    }
}