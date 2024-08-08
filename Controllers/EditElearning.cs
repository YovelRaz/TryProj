using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;
using FinalDbRepository;
using GuideDogsPlatformWback.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml;
using System;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Dapper;

namespace GuideDogsPlatformWback.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditElearning : ControllerBase
    {
        private readonly DbRepository _db;

        public EditElearning(DbRepository db)
        {
            _db = db;
        }


        
        [HttpGet("PageInfo")]
        public async Task<IActionResult> GetPageInfo(int pageId)
        {
            string query = "SELECT Page_Code, Page_Content FROM Pages WHERE Page_id = @pageId";
            var param = new { pageId };

            var pageRecords = await _db.GetRecordsAsync<PageContentUpdateDto>(query, param);

            if (pageRecords != null && pageRecords.Any())
            {
                var pageInfoList = pageRecords.Select(pageRecord => new
                {
                    PageId = pageId,
                    PageCode = pageRecord.Page_Code,
                    PageContent = pageRecord.Page_Content
                }).ToList();

                return Ok(pageInfoList);
            }
            else
            {
                return NotFound();
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



        //        [HttpPost("duplicateUnit")]
        //        public async Task<IActionResult> DuplicateUnit()
        //        {
        //            try
        //            {
        //                // Get the details of the unit to duplicate
        //                var unitToDuplicateQuery = "SELECT Unit_id,Unit_name,Category_name," +
        //                    "Unit_Description, Unit_UpdateDate FROM Units WHERE Unit_id = @UnitId";
        //                var param = new { UnitId = 1 };  // Unit ID to duplicate

        //                var unitToDuplicateRecords = await _db.GetRecordsAsync<UnitDTO>(unitToDuplicateQuery, param);

        //                if (unitToDuplicateRecords == null || !unitToDuplicateRecords.Any())
        //                {
        //                    return BadRequest("Unit with Unit_id = 1 not found");
        //                }

        //                // Create a new unit with the same Unit_name (adjust as necessary)
        //                var newUnitToAdd = new { Unit_name = unitToDuplicateRecords.First().Unit_name };

        //                string insertUnitQuery = "INSERT INTO Units (Unit_name, Unit_UpdateDate) VALUES (@Unit_name, @Unit_UpdateDate)";

        //                // Insert the new unit and return its ID
        //                int newUnitId = await _db.InsertReturnId(insertUnitQuery, new
        //                {
        //                    Unit_name = newUnitToAdd.Unit_name,
        //                    Unit_UpdateDate = DateTime.Now // Set current date for Unit_UpdateDate
        //                });

        //                if (newUnitId != 0) // Check if the new unit ID is not 0
        //                {
        //                    // Duplicate pages associated with the original unit to the new unit
        //                    var originalPagesQuery = "SELECT * FROM Pages WHERE Unit_id = @OriginalUnitId";
        //                    var originalPages = await _db.GetRecordsAsync<PageContentUpdateDto>(originalPagesQuery, new { OriginalUnitId = 1 }); // Adjust if needed

        //                    foreach (var originalPage in originalPages)
        //                    {
        //                        string insertPageQuery = @"
        //INSERT INTO Pages (Page_Title, Page_Code, Update_Date, Page_Content, Unit_id)
        //VALUES (@Page_Title, @Page_Code,  datetime('now'), @Page_Content, @NewUnitId)";

        //                        var insertPageParams = new
        //                        {
        //                            Page_Title = originalPage.Page_Title,
        //                            Page_Code = originalPage.Page_Code,
        //                            Page_Content = originalPage.Page_Content,
        //                            NewUnitId = newUnitId
        //                        };

        //                        await _db.SaveDataAsync(insertPageQuery, insertPageParams);
        //                    }

        //                    return Ok(new { unitId = newUnitId, message = "Unit duplicated successfully" });
        //                }
        //                else
        //                {
        //                    return BadRequest("Failed to duplicate unit");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Log the exception for debugging purposes
        //                Console.WriteLine($"An error occurred: {ex}");

        //                // Return an appropriate error response
        //                return StatusCode(500, $"An error occurred while duplicating unit and associated pages: {ex.Message}");
        //            }
        //        }




        [HttpPost("duplicateUnit")]
        public async Task<IActionResult> DuplicateUnit()
        {
            try
            {
                // Get the details of the unit to duplicate
                var unitToDuplicateQuery = "SELECT Unit_id, Unit_name, Category_name, Unit_Description, Unit_UpdateDate FROM Units WHERE Unit_id = @UnitId";
                var param = new { UnitId = 1 };  // Unit ID to duplicate

                var unitToDuplicateRecords = await _db.GetRecordsAsync<UnitDTO>(unitToDuplicateQuery, param);

                if (unitToDuplicateRecords == null || !unitToDuplicateRecords.Any())
                {
                    return BadRequest("Unit with Unit_id = 1 not found");
                }

                // Create a new unit with the same Unit_name (adjust as necessary)
                var newUnitToAdd = new { Unit_name = unitToDuplicateRecords.First().Unit_name };

                string insertUnitQuery = "INSERT INTO Units (Unit_name, Unit_UpdateDate) VALUES (@Unit_name, @Unit_UpdateDate)";

                // Insert the new unit and return its ID
                int newUnitId = await _db.InsertReturnId(insertUnitQuery, new
                {
                    Unit_name = newUnitToAdd.Unit_name,
                    Unit_UpdateDate = DateTime.Now // Set current date for Unit_UpdateDate
                });

                if (newUnitId != 0) // Check if the new unit ID is not 0
                {
                    // Duplicate pages associated with the original unit to the new unit
                    var originalPagesQuery = "SELECT * FROM Pages WHERE Unit_id = @OriginalUnitId";
                    var originalPages = await _db.GetRecordsAsync<PageContentUpdateDto>(originalPagesQuery, new { OriginalUnitId = 1 }); // Adjust if needed

                    foreach (var originalPage in originalPages)
                    {
                        string insertPageQuery = @"
INSERT INTO Pages (Page_Title, Page_Code, Update_Date, Page_Content, Unit_id, HtmlName)
VALUES (@Page_Title, @Page_Code, datetime('now'), @Page_Content, @NewUnitId, @HtmlName)";

                        var insertPageParams = new
                        {
                            Page_Title = originalPage.Page_Title,
                            Page_Code = originalPage.Page_Code,
                            Page_Content = originalPage.Page_Content,
                            NewUnitId = newUnitId,
                            HtmlName = originalPage.HtmlName // Ensure this matches the column name in your Pages table
                        };

                        await _db.SaveDataAsync(insertPageQuery, insertPageParams);
                    }

                    return Ok(new { unitId = newUnitId, message = "Unit duplicated successfully" });
                }
                else
                {
                    return BadRequest("Failed to duplicate unit");
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                Console.WriteLine($"An error occurred: {ex}");

                // Return an appropriate error response
                return StatusCode(500, $"An error occurred while duplicating unit and associated pages: {ex.Message}");
            }
        }



        [HttpGet("UnitID")]
        public async Task<IActionResult> getAllUnitPage(int myUnit)
        {
            string GetUnitPages = "SELECT * FROM Pages WHERE Unit_id = @myUnit";
            var paramUnit = new
            {
                myUnit = myUnit
            };
            var records = await _db.GetRecordsAsync<PageContentUpdateDto>(GetUnitPages, paramUnit);

            if (records.Any())
            {
                return Ok(records); // Return the list of PageContentUpdateDto objects
            }
            else
            {
                return NotFound();
            }
        }



        [HttpGet("GetNextPageId")]
        public async Task<IActionResult> GetNextPageId(int currentUnitId, int currentPageId)
        {
            string query = @"
        SELECT Page_id FROM Pages
        WHERE Unit_id = @currentUnitId AND Page_id > @currentPageId
        ORDER BY Page_id ASC";
            var nextPage = await _db.GetRecordsAsync<int>(query, new { currentUnitId, currentPageId });

            return nextPage.Any() ? Ok(nextPage.First()) : NotFound("No next page available.");
        }

        [HttpGet("GetPreviousPageId")]
        public async Task<IActionResult> GetPreviousPageId(int currentUnitId, int currentPageId)
        {
            string query = @"
        SELECT Page_id FROM Pages
        WHERE Unit_id = @currentUnitId AND Page_id < @currentPageId
        ORDER BY Page_id DESC";
            var previousPage = await _db.GetRecordsAsync<int>(query, new { currentUnitId, currentPageId });

            return previousPage.Any() ? Ok(previousPage.First()) : NotFound("No previous page available.");
        }



        [HttpGet("GetPageIdByHtmlName")]
        public async Task<IActionResult> GetPageIdByHtmlName(int unitId, string htmlName)
        {
            string query = @"
        SELECT Page_id FROM Pages
        WHERE Unit_id = @unitId AND HtmlName = @htmlName";
            var pageId = await _db.GetRecordsAsync<int>(query, new { unitId, htmlName });

            return pageId.Any() ? Ok(pageId.First()) : NotFound("Page not found.");
        }


    }
}

