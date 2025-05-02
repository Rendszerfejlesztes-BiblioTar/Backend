using BiblioBackend.DataContext.Entities;

namespace BiblioBackend.DataContext.Dtos;

public class BookQualityDto
{
    public int Id { get; set; }
    public BookQuality Quality { get; set; }
}