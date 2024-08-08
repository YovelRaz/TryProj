using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GuideDogsPlatformWback.DTOs;
using TriangleFileStorage;
using FinalDbRepository;

namespace Prog3_WebApi_Javascript.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly FilesManage _filesManage;
        private readonly DbRepository _db;
        private readonly string audioFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "audio");
        private readonly string audioFileName = "story"; // Name of the audio file without extension

        public MediaController(FilesManage filesManage, DbRepository db)
        {
            _filesManage = filesManage;
            _db = db;
        }

        [HttpGet("GetMedia")]
        public async Task<IActionResult> GetMedia()
        {
            try
            {
                string query = "SELECT * FROM Media";
                var param = new { };

                var allMedia = await _db.GetRecordsAsync<MediaDTO>(query, param);

                if (allMedia == null)
                {
                    return NotFound("No media was found");
                }

                return Ok(allMedia);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, "An error occurred while fetching the media.");
            }
        }
        [HttpGet("GetAudio")]
        public async Task<IActionResult> GetAudio()
        {
            try
            {
                string query = "SELECT * FROM Audio";
                var param = new { };

                var audios = await _db.GetRecordsAsync<AudioDto>(query, param);

                if (audios == null)
                {
                    return NotFound("No audio was found");
                }

                return Ok(audios);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex}");
                return StatusCode(500, "An error occurred while fetching the audio.");
            }
        }


        [HttpPost("replaceimage/{id}")]
        public async Task<IActionResult> ReplaceImage(int id, IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            try
            {
                // Fetch the current image URL from the database
                string getCurrentUrlQuery = "SELECT ImageURL FROM Media WHERE Media_id = @Id";
                var currentUrlParam = new { Id = id };
                var currentImageUrlRecords = await _db.GetRecordsAsync<string>(getCurrentUrlQuery, currentUrlParam);
                string currentImageUrl = currentImageUrlRecords.FirstOrDefault();

                if (string.IsNullOrEmpty(currentImageUrl))
                {
                    return NotFound($"No media found with ID {id}");
                }

                // Extract the file name from the URL
                string fileName = Path.GetFileName(currentImageUrl);

                // Construct the full path to the current image file
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                string currentFilePath = Path.Combine(uploadsFolder, fileName);

                // Log the paths for debugging
                Console.WriteLine($"Current Image URL: {currentImageUrl}");
                Console.WriteLine($"File Path to Delete: {currentFilePath}");

                // Check if the current image file exists and delete it
                if (System.IO.File.Exists(currentFilePath))
                {
                    System.IO.File.Delete(currentFilePath);
                    Console.WriteLine($"Deleted old image: {currentFilePath}");
                }
                else
                {
                    Console.WriteLine($"File not found: {currentFilePath}");
                }

                // Save the new image file with the same name
                using (var fileStream = new FileStream(currentFilePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                    Console.WriteLine($"Saved new image: {currentFilePath}");
                }

                return Ok(new { message = "Image replaced successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Error replacing image: {ex.Message}");
            }
        }


        [HttpPost("updateMediaDetails")]
        public async Task<IActionResult> UpdateMediaDetails(MediaDTO media)
        {
            if (media == null)
            {
                return BadRequest("Media data is required.");
            }

            try
            {
                string updateQuery = @"
                UPDATE Media 
                SET ImageTitle = @ImageTitle, ImageAlt = @ImageAlt 
                WHERE Media_id = @Media_id";

                var param = new
                {
                    media.ImageTitle,
                    media.ImageAlt,
                    media.Media_id
                };

                var result = await _db.SaveDataAsync(updateQuery, param);

                if (result)
                {
                    return Ok(new { message = "Media details updated successfully" });
                }
                else
                {
                    return NotFound($"No media found with ID {media.Media_id}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Error updating media details: {ex.Message}");
            }
        }



        //[HttpPost("upload")]
        //public async Task<IActionResult> UploadFile([FromBody] string imageBase64)
        //{
        //    string fileName = await _filesManage.SaveFile(imageBase64, "png", "uploadedFiles");
        //    return Ok(fileName);
        //}


        //[HttpPost("deleteMedia")]
        //public async Task<IActionResult> DeleteMedia(List<string> images)
        //{
        //    var countFalseTry = 0;
        //    foreach (var img in images) // Use the correct property name
        //    {
        //        if (!_filesManage.DeleteFile(img, "")) // Fixed method call
        //        {
        //            countFalseTry++;
        //        }
        //    }

        //    if (countFalseTry > 0)
        //    {
        //        // To the editor: There is no access to the server, and nothing can be fixed. This check is for testing purposes.
        //        return BadRequest("Problem with " + countFalseTry.ToString() + " images");
        //    }

        //    return Ok("Deleted");
        //}

        [HttpPost("replaceaudio")]
        public async Task<IActionResult> ReplaceAudio(IFormFile audioFile)
        {
            if (audioFile == null || audioFile.Length == 0)
            {
                return BadRequest("Audio file is required.");
            }

            try
            {
                // Create the audio folder if it doesn't exist
                if (!Directory.Exists(audioFolder))
                {
                    Directory.CreateDirectory(audioFolder);
                }

                // Delete the existing audio files
                string[] supportedExtensions = new[] { ".ogg", ".mp3" };
                foreach (var ext in supportedExtensions)
                {
                    string currentFilePath = Path.Combine(audioFolder, $"{audioFileName}{ext}");
                    if (System.IO.File.Exists(currentFilePath))
                    {
                        System.IO.File.Delete(currentFilePath);
                        Console.WriteLine($"Deleted old audio: {currentFilePath}");
                    }
                }

                // Save the new audio file
                string newFileExtension = Path.GetExtension(audioFile.FileName);
                if (!supportedExtensions.Contains(newFileExtension))
                {
                    return BadRequest($"Unsupported audio format. Supported formats are: {string.Join(", ", supportedExtensions)}");
                }

                string newFilePath = Path.Combine(audioFolder, $"{audioFileName}{newFileExtension}");
                using (var fileStream = new FileStream(newFilePath, FileMode.Create))
                {
                    await audioFile.CopyToAsync(fileStream);
                    Console.WriteLine($"Saved new audio: {newFilePath}");
                }

                return Ok(new { message = "Audio replaced successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, $"Error replacing audio: {ex.Message}");
            }
        }
    }
}