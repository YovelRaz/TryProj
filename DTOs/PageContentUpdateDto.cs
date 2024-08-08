namespace GuideDogsPlatformWback.DTOs
{
    public class PageContentUpdateDto
    {
        public int Page_id { get; set; }
        public string Page_Title { get; set; }
        public string Page_Code { get; set; }
        public DateTime Update_Date { get; set; }
        public string Page_Content { get; set; }
        public int Unit_id { get; set; }
        public string HtmlName { get; set; }

    }
    public class PageUpdateRequestDto
    {
        public int Page_id { get; set; }
        public string Page_Content { get; set; }
        public DateTime Update_Date { get; set; }
    }

    public class UnitDTO
    {
        public int Unit_id { get; set; }
        public string Unit_name { get; set; }
        public string Category_name { get; set; }
        public string Unit_Description { get; set; }
        public DateTime Unit_UpdateDate { get; set; } // New property for update date

    }

    public class MediaDTO
    {
        public int Media_id { get; set; }
        public string ImageAlt { get; set; }
        public string ImageTitle { get; set; }
        public string ImageURL { get; set; }
        
    }
    public class AudioDto
    {
        public int MediaId { get; set; }
        public string AudioTitle { get; set; }
        public string AudioURL { get; set; }
    }




}