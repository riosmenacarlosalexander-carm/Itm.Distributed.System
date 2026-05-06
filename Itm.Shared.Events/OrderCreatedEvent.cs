using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Itm.Shared.Events;

public record OrderCreatedEvent(
    Guid OrderId,
    int ProductId,
    string UserEmail,
    decimal TotalAmount
    );