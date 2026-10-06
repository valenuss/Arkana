namespace Arkana.Models;

public class Follow
{
    public int Id { get; set; }
    public int FollowerId { get; set; }    // el que sigue
    public int FollowingId { get; set; }   // al que siguen
}