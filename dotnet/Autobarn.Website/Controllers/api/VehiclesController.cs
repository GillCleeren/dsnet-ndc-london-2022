﻿using System;
using Autobarn.Data;
using Autobarn.Data.Entities;
using Autobarn.Website.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autobarn.Website.Controllers.api {
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase {
        private readonly IAutobarnDatabase db;

        public VehiclesController(IAutobarnDatabase db) {
            this.db = db;
        }

        // GET: api/vehicles
        [HttpGet]
        public IActionResult Get(int index = 0, int count = 10) {
            var items = db.ListVehicles().Skip(index).Take(count);
            var total = db.CountVehicles();
            var result = new {
                _links = Hal.Paginate("/api/vehicles", index, count, total),
                items = items.Select(v => v.ToResource())
            };
            return Ok(result);
        }

        // GET api/vehicles/ABC123
        [HttpGet("{id}")]
        public IActionResult Get(string id) {
            var vehicle = db.FindVehicle(id);
            if (vehicle == default) return NotFound();
            dynamic result = vehicle.ToResource();
            result._actions = new {
                update = new {
                    href = $"/api/vehicles/{id}",
                    method = "PUT",
                    name = "Replace/overwrite this vehicle",
                    type = "application/json",
                    schema = "https://developer.autobarn.com/docs/schema/vehicles.xml"
                },
                delete = new {
                    href = $"/api/vehicles/{id}",
                    method = "DELETE",
                    name = "Delete this vehicle"
                }
            };
            return Ok(result);
        }

        // PUT api/vehicles/ABC123
        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] VehicleDto dto) {
            if (!dto.Registration.Equals(id, StringComparison.InvariantCultureIgnoreCase))
                return Conflict("You can't replace that car with this car! (different registration codes)");
            var vehicleModel = db.FindModel(dto.ModelCode);
            var vehicle = new Vehicle {
                Registration = id,
                Color = dto.Color,
                Year = dto.Year,
                ModelCode = vehicleModel.Code
            };
            db.UpdateVehicle(vehicle);
            return Ok(dto);
        }

        // DELETE api/vehicles/ABC123
        [HttpDelete("{id}")]
        public IActionResult Delete(string id) {
            var vehicle = db.FindVehicle(id);
            if (vehicle == default) return NotFound();
            db.DeleteVehicle(vehicle);
            return NoContent();
        }
    }
}
