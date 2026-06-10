using System.Collections.Generic;
using UnityEngine;

/// <summary>One spoken line in a story conversation.</summary>
[System.Serializable]
public class StoryLine
{
    public string speaker;
    [TextArea] public string text;

    public StoryLine() { }

    public StoryLine(string speaker, string text)
    {
        this.speaker = speaker;
        this.text = text;
    }
}

/// <summary>An ordered list of lines that make up one conversation beat.</summary>
public class StoryConversation
{
    public string id;
    public List<StoryLine> lines;

    public StoryConversation(string id, List<StoryLine> lines)
    {
        this.id = id;
        this.lines = lines;
    }
}

/// <summary>
/// The three story conversations, transcribed from the design doc.
/// Speaker names must match the DialogueCharacter titles used for icon lookup.
/// </summary>
public static class StoryScript
{
    public const string Goggle = "Goggle";
    public const string Heiqi = "Heiqi";

    public const string IntroId = "intro";
    public const string AfterFirstLessonsId = "after_first_lessons";
    public const string AfterTrainingHubId = "after_training_hub";

    // [after player first loads into game and clicks start]
    public static StoryConversation Intro()
    {
        return new StoryConversation(IntroId, new List<StoryLine>
        {
            new StoryLine(Goggle, "Heiqi! Heiqi! The emperor is looking for you! He's recruiting you!"),
            new StoryLine(Heiqi, "What the... who are you? Also, I'm already in the army..."),
            new StoryLine(Goggle, "My name is Goggle, one of the emperor's assistants, but this isn't about me! Do you see that castle in the distance?"),
            new StoryLine(Heiqi, "Of course. That's the White Stones Kingdom, our main rival. Everyone here knows that."),
            new StoryLine(Goggle, "And do you see the spreading white things?"),
            new StoryLine(Heiqi, "Yeah, what's up with that?"),
            new StoryLine(Goggle, "We've got word from the other kingdoms that its emperor, Baiqi, found a way to make his soldiers invincible to attacks! He is invading all the nearby villages now!"),
            new StoryLine(Heiqi, "Impossible. There's got to be a catch, right??"),
            new StoryLine(Goggle, "Yes, the only way to remove their invincibility is to defeat them in a game of Go!"),
            new StoryLine(Heiqi, "Go? You mean that ancient board game mastered by only the wisest?"),
            new StoryLine(Goggle, "Yes, and that's why the emperor wants you in his new \"Go army\"! Full of warriors who are both intelligent to beat their army at Go and strong to defeat them in battle after."),
            new StoryLine(Heiqi, "But... I've never played Go before!"),
            new StoryLine(Goggle, "That's alright! We dug up a lot of old tomes from the emperor palace library that can help new learners. Some have already been placed in the Philosophy Academy!"),
            new StoryLine(Heiqi, "I'll be heading over then, thank you."),
            new StoryLine(Goggle, "No problem! Meet me outside after you complete the first three lessons!"),
        });
    }

    // [after the first 3 lessons (basic rules, liberties 1, liberties 2)]
    public static StoryConversation AfterFirstLessons()
    {
        return new StoryConversation(AfterFirstLessonsId, new List<StoryLine>
        {
            new StoryLine(Goggle, "Hello again Heiqi, how are you feeling?"),
            new StoryLine(Heiqi, "Not bad. I see a lot of potential from those rules."),
            new StoryLine(Goggle, "Good to hear! By the way, you've seen the Training Hub, right?"),
            new StoryLine(Heiqi, "Yeah. Didn't they stop using it a while ago?"),
            new StoryLine(Goggle, "We repurposed it into a place to practice your Go skills with puzzles!"),
            new StoryLine(Heiqi, "Woah, that's great. May I try it out?"),
            new StoryLine(Goggle, "You can practice a little in Daily Puzzles or Puzzle Leveling, but there may be some concepts or techniques you haven't learned yet..."),
            new StoryLine(Goggle, "You can definitely take a look inside, though!"),
        });
    }

    // [after exiting Training Hub]
    public static StoryConversation AfterTrainingHub()
    {
        return new StoryConversation(AfterTrainingHubId, new List<StoryLine>
        {
            new StoryLine(Heiqi, "Hey Goggle, I looked inside the Puzzle Leveling. What does the k's mean?"),
            new StoryLine(Goggle, "Glad you asked! They are the skill level ratings, and \"k\" stands for Kyu. The level goes from 18 Kyu to 1 Kyu."),
            new StoryLine(Goggle, "The lower the Kyu, the more skilled the player. After a player achieves the 1 Kyu rating, they can advance to \"d\", standing for \"Dan\"."),
            new StoryLine(Goggle, "It goes from 1 Dan to 9 Dan. The higher the Dan, the more skilled the player, opposite to Kyu."),
            new StoryLine(Heiqi, "Why does it stop at 9 Dan?"),
            new StoryLine(Goggle, "9 Dan is the highest skill a player can achieve. At that level, players can plan forward tens, if not hundreds of moves, spot the opponent's weakness instantly, and so much more!"),
            new StoryLine(Heiqi, "Wow, looks like I have a long way ahead of me then."),
            new StoryLine(Goggle, "Yes, and by the way- I received a message from the emperor while you were in the Training Hub, that he needs me back at the palace..."),
            new StoryLine(Heiqi, "It's all fine. Go quickly or else the emperor might get mad."),
            new StoryLine(Goggle, "Sorry I could only stay for so long. Do well in your training, and may we meet again!"),
            new StoryLine(Heiqi, "See you!"),
        });
    }
}
