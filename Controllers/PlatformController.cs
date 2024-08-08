using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using FinalDbRepository;
using GuideDogsPlatformWback.DTOs;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Prog3_WebApi_Javascript.Controllers
{
    [Route("api/[controller]")]
    public class PlatformController : Controller
    {
        private readonly DbRepository _db;

        public PlatformController(DbRepository db)
        {
            _db = db;
        }



        [HttpGet("GetAllUnitsWithPages")]
        public async Task<IActionResult> GetAllUnitsWithPages()
        {
            try
            {
                string unitsQuery = @"
            SELECT 
                u.Unit_id, 
                u.Unit_name,
                MAX(p.Update_Date) AS Unit_UpdateDate
            FROM 
                Units u
            LEFT JOIN 
                Pages p ON u.Unit_id = p.Unit_id
            GROUP BY 
                u.Unit_id, 
                u.Unit_name
            ORDER BY 
                Unit_UpdateDate DESC";
                var param = new { };

                var units = await _db.GetRecordsAsync<UnitDTO>(unitsQuery, param);

                var unitsWithPages = new List<object>();

                foreach (var unit in units)
                {
                    string pagesQuery = "SELECT Page_id, Page_Title, Page_Code, Update_Date, Page_Content FROM Pages WHERE Unit_id = @Unit_id";
                    var pages = await _db.GetRecordsAsync<PageContentUpdateDto>(pagesQuery, new { Unit_id = unit.Unit_id });

                    unitsWithPages.Add(new
                    {
                        unit.Unit_id,
                        unit.Unit_name,
                        unit.Unit_UpdateDate, // Ensure this line is included and correct
                        Pages = pages
                    });
                }

                return Ok(unitsWithPages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, "An error occurred while fetching the units and pages.");
            }
        }




        [HttpGet("GetUnitSettings")] // Corrected endpoint name
        public async Task<IActionResult> GetUnitSettings(int unitid) // Corrected parameter name
        {
            try
            {
                string settingsQuery = "SELECT * FROM Units WHERE Unit_id = @UnitId";
                var param = new { UnitId = unitid };

                var unitSettings = await _db.GetRecordsAsync<UnitDTO>(settingsQuery, param);

                if (unitSettings == null)
                {
                    return NotFound($"No unit found with ID {unitid}");
                }

                return Ok(unitSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, "An error occurred while fetching the unit settings.");
            }
        }

        [HttpPost("UpdateUnit")]
        public async Task<IActionResult> UpdateUnitSettings([FromBody] UnitDTO data)
        {
            try
            {
                var newData = new
                {
                    data.Unit_id,
                    data.Unit_name,
                    data.Unit_Description,
                    data.Category_name,
                };

                string updateQuery = "UPDATE Units SET Unit_name = @Unit_name, Unit_Description = @Unit_Description, Category_name = @Category_name WHERE Unit_id = @Unit_id";
                bool isUpdate = await _db.SaveDataAsync(updateQuery, newData);

                if (isUpdate)
                {
                    string unitQuery = "SELECT Unit_id, Unit_name, Unit_Description, Category_name FROM Units WHERE Unit_id = @Unit_id";
                    var unitsRecords = await _db.GetRecordsAsync<UnitDTO>(unitQuery, newData);
                    UnitDTO updatedUnit = unitsRecords.FirstOrDefault();

                    return Ok(updatedUnit);
                }
                else
                {
                    return BadRequest("Update Failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, "An error occurred while updating the unit settings.");
            }
        }










        [HttpDelete("DeleteUnit/{idToDelete}")]
        public async Task<IActionResult> DeleteUnit(int idToDelete)
        {
            if (idToDelete == 1)
            {
                return BadRequest("Unit with unit_id equal to 1 cannot be deleted.");
            }

            object param = new
            {
                Unit_id = idToDelete
            };

            string pageDeleteQuery = "DELETE FROM Pages WHERE Unit_id=@Unit_id";
            string unitDeleteQuery = "DELETE FROM Units WHERE Unit_id=@Unit_id";

            try
            {
                // Delete the pages associated with the unit
                bool arePagesDeleted = await _db.SaveDataAsync(pageDeleteQuery, param);
                if (!arePagesDeleted)
                {
                    return StatusCode(500, "Failed to delete pages");
                }

                // Delete the unit
                bool isUnitDeleted = await _db.SaveDataAsync(unitDeleteQuery, param);
                if (isUnitDeleted)
                {
                    return Ok("Unit and associated pages deleted successfully");
                }
                else
                {
                    return StatusCode(500, "Failed to delete unit");
                }
            }
            catch (Exception ex)
            {
                // Use a proper logging framework
                Console.WriteLine(ex);
                return StatusCode(500, "An error occurred while processing the request");
            }
        }



        [HttpGet("GetPagesByUnitId")]
        public async Task<IActionResult> GetPagesByUnitId(int unitId)
        {
            try
            {
                string pagesQuery = @"
            SELECT Page_id, Page_Title, Page_Code, Update_Date, Page_Content 
            FROM Pages 
            WHERE Unit_id = @UnitId
            ORDER BY Page_id ASC";

                var pages = await _db.GetRecordsAsync<PageContentUpdateDto>(pagesQuery, new { UnitId = unitId });

                if (pages == null || !pages.Any())
                {
                    return NotFound($"No pages found for unit ID {unitId}");
                }

                return Ok(pages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, "An error occurred while fetching the pages.");
            }
        }




        [HttpGet("GetUnitsByUpdateDate")]
        public async Task<IActionResult> GetUnitsByUpdateDate()
        {
            try
            {
                object param = new
                {
                };

                string unitDateQuery = @"
            SELECT 
                u.Unit_id, 
                u.Unit_name, 
                MAX(p.Update_Date) AS Latest_Update_Date
            FROM Units u
            JOIN Pages p ON u.Unit_id = p.Unit_id
            GROUP BY u.Unit_id, u.Unit_name
            ORDER BY p.Update_Date DESC";

                var units = await _db.GetRecordsAsync<UnitDTO>(unitDateQuery, param);

                return Ok(units);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, "An error occurred while fetching the units.");
            }
        }



        [HttpPost("UpdatePage")]
        public async Task<IActionResult> UpdateThePage(PageUpdateRequestDto data)
        {
            try
            {
                // Prepare anonymous object with properties matching the database columns
                var newData = new
                {
                    Page_id = data.Page_id,
                    Page_Content = data.Page_Content,
                    Update_Date = DateTime.Now,
                };

                // Log the data being sent to the server
                Console.WriteLine("Data sent to server:");
                Console.WriteLine($"Page Content: {newData.Page_Content}");
                Console.WriteLine($"Update Date: {newData.Update_Date}");

                // SQL query with parameters
                string updateQuery = "UPDATE Pages SET Page_Content = @Page_Content, Update_Date = @Update_Date WHERE Page_id = @Page_id";
                bool isUpdate = await _db.SaveDataAsync(updateQuery, newData); // Check if the update was successful

                if (isUpdate)
                {
                    // Update the update date of the unit associated with the page
                    string updateUnitQuery = "UPDATE Units SET Update_Date = @Update_Date WHERE Unit_id = (SELECT Unit_id FROM Pages WHERE Page_id = @Page_id)";
                    await _db.SaveDataAsync(updateUnitQuery, newData);

                    string pageQuery = "SELECT Page_id,Page_Content,Update_Date FROM Pages WHERE Page_id = @Page_id";
                    var PagesRecords = await _db.GetRecordsAsync<PageContentUpdateDto>(pageQuery, newData);
                    PageContentUpdateDto updatedPage = PagesRecords.FirstOrDefault();

                    // Return the updatedPage object directly
                    return Ok(updatedPage);
                }
                else
                {
                    return BadRequest("Update Failed");
                }

            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred: {ex}");

                // Return an appropriate error response
                return StatusCode(500, "An error occurred while updating the page.");
            }
        }






    }
}

