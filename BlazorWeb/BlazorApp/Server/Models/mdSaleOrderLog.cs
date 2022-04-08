using MongoDB.Bson;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp.Server.Models
{
    [Collection("SaleOrderLog")]
    public class mdSaleOrderLog : mdSaleOrder
    {
    }



}
