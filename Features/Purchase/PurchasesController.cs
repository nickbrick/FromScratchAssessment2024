using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FromScratchAssessment2024;

namespace FromScratchAssessment2024
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly PurchasesRepository _purchasesRepository;

        public PurchasesController(PurchasesRepository purchasesRepository)
        {
            _purchasesRepository = purchasesRepository;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Purchase>>> Get()
        {
            var purchases = await _purchasesRepository.GetAllAsync();
            return Ok(purchases);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Purchase>> Get(Guid id)
        {
            var purchase = await _purchasesRepository.GetByIdAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }
            return Ok(purchase);
        }

        [HttpPost]
        public async Task<ActionResult<Purchase>> Create(Purchase purchase)
        {
            if (purchase.PurchaseItems.Count == 0)
                return BadRequest("PurchaseItems was empty.");
            var insertedPurchase = await _purchasesRepository.AddAsync(purchase);
            if (insertedPurchase == null)
                return BadRequest("PurchaseItems contains no valid items.");
            return CreatedAtAction(nameof(Get), new { id = purchase.Id }, insertedPurchase);
        }
    }
}
