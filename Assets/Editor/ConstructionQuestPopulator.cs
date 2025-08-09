using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LifeCraft.Systems;

public class ConstructionQuestPopulator : EditorWindow
{
    private ConstructionQuestPool questPool;
    private string questPoolPath = "Assets/Resources/ConstructionQuestPool.asset";

    [MenuItem("LifeCraft/Populate Construction Quests")]
    public static void ShowWindow()
    {
        GetWindow<ConstructionQuestPopulator>("Construction Quest Populator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Populate Construction Quest Pool", EditorStyles.boldLabel);

        questPool = (ConstructionQuestPool)EditorGUILayout.ObjectField("Quest Pool Asset", questPool, typeof(ConstructionQuestPool), false);

        if (questPool == null)
        {
            EditorGUILayout.HelpBox("Please assign the ConstructionQuestPool asset.", MessageType.Warning);
            if (GUILayout.Button("Load Default Quest Pool"))
            {
                questPool = AssetDatabase.LoadAssetAtPath<ConstructionQuestPool>(questPoolPath);
                if (questPool == null)
                {
                    Debug.LogError($"ConstructionQuestPool asset not found at {questPoolPath}. Please create it first.");
                }
            }
        }
        else
        {
            if (GUILayout.Button("Generate and Populate Quests"))
            {
                PopulateQuests();
                EditorUtility.SetDirty(questPool);
                AssetDatabase.SaveAssets();
                Debug.Log("Construction Quest Pool populated successfully!");
            }

            if (GUILayout.Button("Clear All Quests"))
            {
                ClearAllQuests();
                EditorUtility.SetDirty(questPool);
                AssetDatabase.SaveAssets();
                Debug.Log("All Construction Quests cleared!");
            }
        }
    }

    private void ClearAllQuests()
    {
        if (questPool == null) return;

        questPool.ClearAllQuests();
    }

    private void PopulateQuests()
    {
        if (questPool == null)
        {
            Debug.LogError("ConstructionQuestPool asset is not assigned.");
            return;
        }

        ClearAllQuests();

        // --- Default Quests (if region not found) ---
        PopulateDefaultQuests();

        // --- Region-Specific Quests ---
        PopulateHealthHarborQuests();
        PopulateMindPalaceQuests();
        PopulateCreativeCommonsQuests();
        PopulateSocialSquareQuests();
    }

    private void PopulateDefaultQuests()
    {
        // Default Easy Quests
        string[] defaultEasyQuests = {
            "Take a 5-minute mindful breathing break.",
            "Spend 10 minutes tidying your workspace.",
            "Drink a glass of water.",
            "Stretch for 2 minutes.",
            "Write down one positive thought.",
            "Take 3 deep breaths.",
            "Stand up and move for 1 minute.",
            "Look out the window for 30 seconds.",
            "Organize one small area of your space.",
            "Practice gratitude by thinking of 3 good things.",
            "Do a quick 2-minute meditation.",
            "Take a short walk around your room.",
            "Drink a cup of herbal tea.",
            "Write down one goal for tomorrow.",
            "Listen to a calming song."
        };

        // Default Medium Quests
        string[] defaultMediumQuests = {
            "Go for a 15-minute walk outside.",
            "Learn a new word or fact.",
            "Connect with a friend or family member.",
            "Try a new healthy recipe.",
            "Spend 30 minutes on a creative hobby.",
            "Read a chapter of a book.",
            "Practice a new skill for 20 minutes.",
            "Plan your meals for the next 3 days.",
            "Do a 15-minute workout session.",
            "Write in your journal for 10 minutes.",
            "Learn something new online.",
            "Try a new form of exercise.",
            "Cook a meal from scratch.",
            "Spend time in nature.",
            "Practice a hobby you enjoy."
        };

        // Default Hard Quests
        string[] defaultHardQuests = {
            "Complete a 30-minute workout session.",
            "Read a chapter of a non-fiction book.",
            "Volunteer for 1 hour in your community.",
            "Meditate for 20 minutes.",
            "Plan your meals for the next 3 days.",
            "Learn a new language for 30 minutes.",
            "Complete a challenging puzzle.",
            "Write a detailed plan for a project.",
            "Practice a musical instrument for 30 minutes.",
            "Research a topic you're curious about.",
            "Take an online course for 1 hour.",
            "Complete a home improvement task.",
            "Learn a new software or tool.",
            "Write a short story or essay.",
            "Practice a new sport or activity."
        };

        // Default Expert Quests
        string[] defaultExpertQuests = {
            "Engage in a challenging learning activity for 1 hour.",
            "Complete a major decluttering task.",
            "Participate in a group fitness class.",
            "Write a short story or poem.",
            "Reflect on your goals and progress for 30 minutes.",
            "Learn a complex skill or technique.",
            "Complete a significant creative project.",
            "Teach someone a new skill.",
            "Plan and execute a major goal.",
            "Research and implement a new habit.",
            "Create a detailed personal development plan.",
            "Complete a challenging physical activity.",
            "Learn and practice a new art form.",
            "Write a comprehensive journal entry.",
            "Develop a new routine or system."
        };

        questPool.DefaultEasyQuests.AddRange(defaultEasyQuests);
        questPool.DefaultMediumQuests.AddRange(defaultMediumQuests);
        questPool.DefaultHardQuests.AddRange(defaultHardQuests);
        questPool.DefaultExpertQuests.AddRange(defaultExpertQuests);
    }

    private void PopulateHealthHarborQuests()
    {
        var healthHarborData = new ConstructionQuestPool.RegionQuestData
        {
            regionType = AssessmentQuizManager.RegionType.HealthHarbor
        };

        // Health Harbor Easy Quests
        healthHarborData.easyQuests.AddRange(new string[] {
            "Do 5 minutes of light stretching.",
            "Drink a full glass of water.",
            "Take a 2-minute mindful breathing break.",
            "Eat a piece of fruit.",
            "Stand up and move for 1 minute.",
            "Do 10 jumping jacks.",
            "Take a 5-minute walk outside.",
            "Practice deep breathing for 2 minutes.",
            "Do some gentle neck stretches.",
            "Drink herbal tea instead of coffee.",
            "Take the stairs instead of the elevator.",
            "Do 5 push-ups or wall push-ups.",
            "Stretch your arms overhead for 30 seconds.",
            "Take a short walk around your building.",
            "Do some shoulder rolls and stretches.",
            "Practice mindful eating for one meal.",
            "Do 10 squats.",
            "Take a 3-minute break to move around.",
            "Do some wrist and finger stretches.",
            "Practice good posture for 10 minutes."
        });

        // Health Harbor Medium Quests
        healthHarborData.mediumQuests.AddRange(new string[] {
            "Go for a 15-minute brisk walk.",
            "Prepare a healthy snack.",
            "Practice deep breathing for 5 minutes.",
            "Do 10 push-ups or squats.",
            "Track your food intake for one meal.",
            "Do a 20-minute workout session.",
            "Cook a healthy meal from scratch.",
            "Practice yoga for 15 minutes.",
            "Go for a bike ride.",
            "Do some strength training exercises.",
            "Plan your meals for tomorrow.",
            "Practice mindful eating for the day.",
            "Do a cardio workout for 20 minutes.",
            "Learn a new healthy recipe.",
            "Practice meditation for 10 minutes.",
            "Do some flexibility exercises.",
            "Go swimming or to the gym.",
            "Practice stress-relief techniques.",
            "Do a home workout routine.",
            "Learn about nutrition and healthy eating."
        });

        // Health Harbor Hard Quests
        healthHarborData.hardQuests.AddRange(new string[] {
            "Complete a 30-minute cardio session.",
            "Cook a healthy, balanced meal from scratch.",
            "Practice yoga or stretching for 20 minutes.",
            "Plan your fitness routine for the week.",
            "Get 7-8 hours of sleep tonight.",
            "Complete a high-intensity workout.",
            "Learn and practice a new sport.",
            "Create a detailed meal plan for the week.",
            "Practice advanced yoga poses.",
            "Complete a long-distance walk or run.",
            "Learn about and implement a new diet.",
            "Practice mindfulness meditation for 20 minutes.",
            "Complete a strength training session.",
            "Learn about nutrition and meal planning.",
            "Practice stress management techniques.",
            "Complete a challenging physical activity.",
            "Learn about and practice proper form.",
            "Create a comprehensive fitness plan.",
            "Practice advanced breathing techniques.",
            "Learn about and implement healthy habits."
        });

        // Health Harbor Expert Quests
        healthHarborData.expertQuests.AddRange(new string[] {
            "Complete a high-intensity interval training (HIIT) workout.",
            "Research and implement a new healthy habit.",
            "Prepare healthy meals for the entire day.",
            "Run or jog for 45 minutes.",
            "Consult a health resource for new insights.",
            "Complete a marathon training session.",
            "Learn and master a new fitness technique.",
            "Create a comprehensive health and fitness plan.",
            "Practice advanced meditation techniques.",
            "Complete a challenging physical challenge.",
            "Learn about and implement a specialized diet.",
            "Master a complex yoga sequence.",
            "Create and follow a detailed nutrition plan.",
            "Practice advanced stress management.",
            "Complete a significant fitness milestone.",
            "Learn about and implement recovery techniques.",
            "Master a new form of exercise.",
            "Create a long-term health and wellness plan.",
            "Practice advanced breathing and meditation.",
            "Achieve a significant health or fitness goal."
        });

        questPool.AddRegionQuestData(healthHarborData);
    }

    private void PopulateMindPalaceQuests()
    {
        var mindPalaceData = new ConstructionQuestPool.RegionQuestData
        {
            regionType = AssessmentQuizManager.RegionType.MindPalace
        };

        // Mind Palace Easy Quests
        mindPalaceData.easyQuests.AddRange(new string[] {
            "Learn one new word today.",
            "Read a short article on a new topic.",
            "Solve a simple puzzle (e.g., Sudoku, crossword).",
            "Spend 5 minutes organizing your digital files.",
            "Recall 3 positive memories from your day.",
            "Learn a new fact about history.",
            "Practice mental math for 5 minutes.",
            "Learn a new vocabulary word.",
            "Read a short story or poem.",
            "Solve a brain teaser.",
            "Learn about a new country or culture.",
            "Practice spelling difficult words.",
            "Learn a new scientific fact.",
            "Read an educational article.",
            "Practice memory exercises.",
            "Learn about a famous person.",
            "Solve a logic puzzle.",
            "Read about a new technology.",
            "Learn a new concept in science.",
            "Practice critical thinking with a simple problem."
        });

        // Mind Palace Medium Quests
        mindPalaceData.mediumQuests.AddRange(new string[] {
            "Watch a short educational video (10-15 min).",
            "Practice a new skill for 15 minutes.",
            "Write down 5 things you're grateful for.",
            "Engage in a conversation about a complex topic.",
            "Review your daily goals for 10 minutes.",
            "Learn basic phrases in a new language.",
            "Read a chapter of a non-fiction book.",
            "Practice problem-solving skills.",
            "Learn about a new field of study.",
            "Write a detailed journal entry.",
            "Study a new subject for 20 minutes.",
            "Practice analytical thinking.",
            "Learn about a new philosophy or theory.",
            "Read an academic article.",
            "Practice creative thinking exercises.",
            "Learn about a new scientific discovery.",
            "Study a new mathematical concept.",
            "Read about a historical event.",
            "Practice logical reasoning.",
            "Learn about a new psychological concept."
        });

        // Mind Palace Hard Quests
        mindPalaceData.hardQuests.AddRange(new string[] {
            "Read a chapter of a non-fiction book.",
            "Learn basic phrases in a new language.",
            "Complete a challenging logic puzzle.",
            "Write a journal entry reflecting on your thoughts.",
            "Research a topic you're curious about for 30 minutes.",
            "Study a complex subject for 1 hour.",
            "Learn and practice a new skill.",
            "Write an essay on a topic of interest.",
            "Complete a challenging brain training exercise.",
            "Learn about advanced concepts in a field.",
            "Practice advanced problem-solving.",
            "Study a new academic discipline.",
            "Learn about complex theories or philosophies.",
            "Complete a challenging educational course.",
            "Practice advanced analytical thinking.",
            "Learn about cutting-edge research.",
            "Study advanced mathematical concepts.",
            "Learn about complex historical events.",
            "Practice advanced logical reasoning.",
            "Learn about advanced psychological theories."
        });

        // Mind Palace Expert Quests
        mindPalaceData.expertQuests.AddRange(new string[] {
            "Dedicate 1 hour to deep work on a complex problem.",
            "Learn a new software or tool.",
            "Write a detailed plan for a personal project.",
            "Engage in a debate or discussion on a philosophical topic.",
            "Teach someone a new concept you've learned.",
            "Master a complex skill or technique.",
            "Complete an advanced educational course.",
            "Write a comprehensive research paper.",
            "Learn and master a new academic discipline.",
            "Solve a complex, multi-step problem.",
            "Create a detailed learning plan.",
            "Master advanced concepts in a field.",
            "Complete a challenging intellectual project.",
            "Learn about and understand complex theories.",
            "Practice advanced critical thinking.",
            "Master a new programming language.",
            "Learn about and implement advanced strategies.",
            "Complete a significant intellectual challenge.",
            "Master advanced analytical techniques.",
            "Achieve a significant learning milestone."
        });

        questPool.AddRegionQuestData(mindPalaceData);
    }

    private void PopulateCreativeCommonsQuests()
    {
        var creativeCommonsData = new ConstructionQuestPool.RegionQuestData
        {
            regionType = AssessmentQuizManager.RegionType.CreativeCommons
        };

        // Creative Commons Easy Quests
        creativeCommonsData.easyQuests.AddRange(new string[] {
            "Doodle for 5 minutes.",
            "Listen to a new genre of music.",
            "Take a photo of something beautiful.",
            "Rearrange a small item in your room.",
            "Write down a creative idea.",
            "Draw a simple picture.",
            "Create a new playlist.",
            "Write a short poem.",
            "Take a creative photo.",
            "Make a small craft project.",
            "Write a creative story idea.",
            "Draw your favorite animal.",
            "Create a mood board.",
            "Write a haiku.",
            "Take an artistic photo.",
            "Make a simple drawing.",
            "Write a creative caption.",
            "Create a new color palette.",
            "Draw a landscape.",
            "Write a creative description."
        });

        // Creative Commons Medium Quests
        creativeCommonsData.mediumQuests.AddRange(new string[] {
            "Spend 15 minutes on a creative hobby (e.g., drawing, writing, music).",
            "Try a new recipe or ingredient.",
            "Write a short poem or haiku.",
            "Brainstorm 10 new uses for a common object.",
            "Create a small piece of digital art.",
            "Learn a new chord on an instrument.",
            "Write a short story outline.",
            "Design a simple logo or icon.",
            "Experiment with a new art medium.",
            "Create a mood board for a project.",
            "Write a creative piece of fiction.",
            "Learn a new drawing technique.",
            "Create a simple animation.",
            "Write a song or melody.",
            "Design a new room layout.",
            "Create a digital collage.",
            "Write a creative blog post.",
            "Learn a new photography technique.",
            "Create a simple sculpture.",
            "Design a new outfit or style."
        });

        // Creative Commons Hard Quests
        creativeCommonsData.hardQuests.AddRange(new string[] {
            "Dedicate 30 minutes to a personal creative project.",
            "Learn a new chord on an instrument.",
            "Write a short story outline.",
            "Design a simple logo or icon.",
            "Experiment with a new art medium.",
            "Create a complex piece of artwork.",
            "Write a detailed creative story.",
            "Learn and master a new creative skill.",
            "Create a comprehensive design project.",
            "Write a complete short story.",
            "Create a complex digital artwork.",
            "Learn and practice a new art form.",
            "Design a complete creative project.",
            "Write a creative screenplay or script.",
            "Create a detailed illustration.",
            "Learn and master a new instrument.",
            "Create a complex creative composition.",
            "Design a comprehensive creative system.",
            "Write a creative novel outline.",
            "Create a detailed creative portfolio."
        });

        // Creative Commons Expert Quests
        creativeCommonsData.expertQuests.AddRange(new string[] {
            "Complete a significant portion of a creative project.",
            "Compose a short piece of music.",
            "Write a detailed script for a short video.",
            "Create a complex digital illustration.",
            "Collaborate on a creative endeavor with someone.",
            "Master a complex creative technique.",
            "Create a comprehensive creative work.",
            "Write and complete a creative story.",
            "Create a detailed creative masterpiece.",
            "Learn and master a new creative discipline.",
            "Create a complex creative composition.",
            "Write a creative novel or book.",
            "Create a detailed creative portfolio.",
            "Master advanced creative techniques.",
            "Create a significant creative achievement.",
            "Learn and implement advanced creative skills.",
            "Create a complex creative system.",
            "Master a new creative medium.",
            "Create a detailed creative masterpiece.",
            "Achieve a significant creative milestone."
        });

        questPool.AddRegionQuestData(creativeCommonsData);
    }

    private void PopulateSocialSquareQuests()
    {
        var socialSquareData = new ConstructionQuestPool.RegionQuestData
        {
            regionType = AssessmentQuizManager.RegionType.SocialSquare
        };

        // Social Square Easy Quests
        socialSquareData.easyQuests.AddRange(new string[] {
            "Send a positive text message to a friend.",
            "Compliment someone today.",
            "Smile at a stranger.",
            "Ask someone about their day.",
            "Share a positive news story.",
            "Thank someone for something they did.",
            "Give someone a genuine compliment.",
            "Ask a coworker how they're doing.",
            "Share a joke or funny story.",
            "Offer to help someone with a small task.",
            "Listen actively to someone's story.",
            "Give someone a small gift or treat.",
            "Ask someone for their opinion.",
            "Share something positive on social media.",
            "Give someone words of encouragement.",
            "Ask someone about their interests.",
            "Share a positive memory with someone.",
            "Give someone a hug (if appropriate).",
            "Ask someone about their goals.",
            "Share a positive quote or message."
        });

        // Social Square Medium Quests
        socialSquareData.mediumQuests.AddRange(new string[] {
            "Call a family member or friend for 10 minutes.",
            "Offer help to someone in need.",
            "Engage in a meaningful conversation.",
            "Connect with someone new online (e.g., professional network).",
            "Send a thank-you note or message.",
            "Plan a small gathering with friends.",
            "Volunteer for a local cause.",
            "Join a new social group or club.",
            "Reach out to an old friend.",
            "Participate in a community event.",
            "Offer to mentor someone.",
            "Organize a small social activity.",
            "Connect with a neighbor.",
            "Join an online community.",
            "Plan a family activity.",
            "Offer emotional support to someone.",
            "Participate in a group discussion.",
            "Connect with a colleague outside work.",
            "Plan a social outing.",
            "Join a new social network."
        });

        // Social Square Hard Quests
        socialSquareData.hardQuests.AddRange(new string[] {
            "Spend 30 minutes catching up with a friend.",
            "Participate in a community event or group.",
            "Offer to mentor or teach someone a skill.",
            "Organize a small social gathering.",
            "Actively listen to someone without interrupting for 15 minutes.",
            "Volunteer for a social cause for 1 hour.",
            "Host a small gathering or virtual meetup.",
            "Initiate a new social connection or collaboration.",
            "Lead a discussion or group activity.",
            "Help resolve a conflict or mediate a discussion.",
            "Organize a community event.",
            "Mentor someone in a skill you know.",
            "Host a social gathering.",
            "Lead a group discussion.",
            "Organize a team-building activity.",
            "Create a new social group or club.",
            "Host a virtual social event.",
            "Organize a community service project.",
            "Lead a social initiative.",
            "Create a social support network."
        });

        // Social Square Expert Quests
        socialSquareData.expertQuests.AddRange(new string[] {
            "Volunteer for a social cause for 1 hour.",
            "Host a small gathering or virtual meetup.",
            "Initiate a new social connection or collaboration.",
            "Lead a discussion or group activity.",
            "Help resolve a conflict or mediate a discussion.",
            "Create and lead a social organization.",
            "Organize a major community event.",
            "Establish a new social program.",
            "Lead a social movement or initiative.",
            "Create a comprehensive social network.",
            "Organize a large social gathering.",
            "Lead a community development project.",
            "Create a social support system.",
            "Establish a mentoring program.",
            "Lead a social advocacy campaign.",
            "Create a community organization.",
            "Lead a social innovation project.",
            "Establish a social enterprise.",
            "Create a comprehensive social program.",
            "Achieve a significant social impact."
        });

        questPool.AddRegionQuestData(socialSquareData);
    }
} 