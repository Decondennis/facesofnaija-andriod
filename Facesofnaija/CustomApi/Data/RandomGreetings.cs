using System;

namespace Facesofnaija.CustomApi.Data
{
    internal class RandomGreetings
    {
        public static Random GenRandom = new Random();
        public static string[] MorningGreetings = {
            "Good morning! May today bring you fresh strength, clear direction, and reasons to smile.",
            "Good morning! Every new day is another opportunity to become better, stronger, and wiser.",
            "Good morning! May the doors you need open, the obstacles before you move, and your efforts produce good results.",
            "Wake up with hope. Yesterday is gone, and today carries new possibilities. Good morning!",
            "Good morning! May you have the courage to pursue your dreams and the patience to trust the process.",
            "Today is a fresh page. Write something beautiful on it. Good morning!",
            "Good morning! May your mind be peaceful, your heart be joyful, and your steps be purposeful.",
            "Rise and shine! You have another opportunity to make progress, no matter how small.",
            "Good morning! May what you couldn't accomplish yesterday become possible today.",
            "Start this day believing that something good can happen. Good morning!",
            "Good morning! Don't worry about having everything figured out. Take the next right step.",
            "A new morning means new strength, new ideas, and new opportunities. Make today count!",
            "Good morning! May your hard work eventually speak louder than every doubt you have faced.",
            "Today, choose hope over fear, courage over doubt, and progress over perfection. Good morning!",
            "Good morning! Your story is still being written. Don't let one difficult chapter define the whole book.",
            "May today remind you that you are capable of more than you think. Good morning!",
            "Good morning! Keep moving forward. Even slow progress is still progress.",
            "Wake up grateful for another chance to try, grow, love, and succeed. Good morning!",
            "Good morning! May unexpected blessings find you and may your efforts bring meaningful results.",
            "Don't allow yesterday's disappointment to steal today's opportunity. Good morning!",
            "Good morning! Trust that better days can still come, even when today begins quietly.",
            "May today bring you wisdom for every decision and strength for every challenge. Good morning!",
            "Good morning! Whatever you face today, remember that you have survived difficult days before.",
            "Start your morning with gratitude, walk through the day with confidence, and end it with satisfaction.",
            "Good morning! May your plans succeed, your worries become lighter, and your heart remain hopeful.",
            "Today doesn't need to be perfect. It only needs to move you forward. Good morning!",
            "Good morning! Believe in the possibilities ahead of you, even when you cannot see the whole path.",
            "May today bring you closer to the things you've been praying, working, and hoping for. Good morning!",
            "Good morning! Never underestimate what one determined person can accomplish in one focused day.",
            "Rise with confidence. You have faced enough challenges to know that you can handle whatever comes your way.",
            "Good morning! May you find strength where you feel weak and clarity where you feel uncertain.",
            "Every sunrise is proof that endings can make room for new beginnings. Good morning!",
            "Good morning! Keep your heart hopeful and your hands busy. Your season of results will come.",
            "Today, don't just wish for a better life—take one step toward creating it. Good morning!",
            "Good morning! May your efforts today plant seeds that produce beautiful results tomorrow.",
            "You may not know what today holds, but you can choose how you meet it. Meet it with courage. Good morning!",
            "Good morning! May peace guard your heart and wisdom guide your decisions throughout the day.",
            "Another morning, another opportunity to get it right. Keep going. Good morning!",
            "Good morning! Don't compare your journey with someone else's. Focus on the road in front of you.",
            "May today bring you closer to your purpose and farther away from unnecessary worry. Good morning!",
            "Good morning! Your current situation is not necessarily your final destination. Keep believing and keep building.",
            "Wake up knowing that you still have something valuable to contribute to the world. Good morning!",
            "Good morning! May your confidence be stronger than your fears and your determination stronger than your excuses.",
            "Today, give yourself permission to start again. There is no shame in rebuilding. Good morning!",
            "Good morning! May opportunities recognize your preparation and may your hard work meet the right doors.",
            "Whatever didn't work yesterday, learn from it and try again today. Good morning!",
            "Good morning! You don't need to see the entire staircase; take the next step.",
            "May your morning begin with peace and your day end with something worth being grateful for.",
            "Good morning! Sometimes the breakthrough begins with simply refusing to give up.",
            "Rise with gratitude. You have another day to change something, improve something, or inspire someone.",
            "Good morning! May your thoughts be positive, your decisions wise, and your actions productive.",
            "Don't let fear make decisions for you today. Be brave enough to pursue what matters. Good morning!",
            "Good morning! May you receive the strength to handle what you cannot change and the wisdom to change what you can.",
            "Today is not a repeat of yesterday. Give it a fresh chance. Good morning!",
            "Good morning! Keep planting good seeds—kindness, discipline, patience, and hard work. They will produce results.",
            "May your efforts be rewarded, your heart remain peaceful, and your hope stay alive. Good morning!",
            "Good morning! The fact that you woke up today means there is still something ahead worth discovering.",
            "Don't count yourself out. Some of your best chapters may still be ahead. Good morning!",
            "Good morning! May today surprise you with good news, genuine smiles, and unexpected opportunities.",
            "Start where you are. Use what you have. Do what you can. Good morning!",
            "Good morning! Your dreams deserve your discipline, not just your wishes.",
            "May today give you another reason to believe that things can get better. Good morning!",
            "Good morning! Whatever challenge comes your way, face it one step, one decision, and one moment at a time.",
            "Don't carry yesterday's burdens into today's opportunities. Let today be today. Good morning!",
            "Good morning! May your journey be guided by wisdom and your destination be worth the effort.",
            "Every morning is a reminder that you have another chance to make a difference. Good morning!",
            "Good morning! Be patient with yourself. Growth takes time, but every effort matters.",
            "May you have the courage to begin, the discipline to continue, and the wisdom to know when to change direction.",
            "Good morning! Don't wait for everything to be perfect before you begin. Start and improve along the way.",
            "Today, focus on what you can control and release what you cannot. Good morning!",
            "Good morning! May your day be productive without being stressful, successful without losing your peace, and busy without losing yourself.",
            "Your past may explain where you came from, but it doesn't have to determine where you're going. Good morning!",
            "Good morning! Keep your vision clear, your heart humble, and your determination strong.",
            "May the right people, opportunities, and ideas find their way into your day. Good morning!",
            "Good morning! Don't be discouraged by how far you have to go. Be encouraged by how far you've already come.",
            "Every great achievement begins with someone deciding not to quit. Keep going. Good morning!",
            "Good morning! May today bring progress in places where you've been waiting patiently.",
            "Your circumstances may change slowly, but your attitude can change this morning. Choose hope. Good morning!",
            "Good morning! Make today meaningful, not merely busy.",
            "May you have enough courage to chase your goals and enough wisdom to appreciate the journey. Good morning!",
            "Good morning! Sometimes the answer is not to work harder but to think differently. Stay open to new possibilities.",
            "You don't have to know exactly how everything will work out. Take the next step with faith and courage. Good morning!",
            "Good morning! May every setback teach you something and every success remind you to remain grateful.",
            "Today, give your best without worrying about being better than everyone else. Good morning!",
            "Good morning! May your quiet efforts eventually become visible results.",
            "Don't allow a difficult season to convince you that your entire life will be difficult. Better days are possible. Good morning!",
            "Good morning! May you find joy in the little things while working toward the bigger things.",
            "You have another opportunity today to become the person you've been working toward. Good morning!",
            "Good morning! Let your actions today be stronger than the excuses that held you back yesterday.",
            "May your day be filled with productive conversations, meaningful opportunities, and peaceful moments. Good morning!",
            "Good morning! Remember: you don't need everyone's approval to pursue what is right for your life.",
            "Start today with a grateful heart and a determined mind. Good morning!",
            "Good morning! May your patience be rewarded, your courage strengthened, and your dreams brought closer.",
            "Don't be afraid of starting small. Great things often begin quietly. Good morning!",
            "Good morning! May you have the wisdom to recognize an opportunity and the courage to act when it appears.",
            "Every day doesn't have to bring a major breakthrough. Sometimes simply continuing is the victory. Good morning!",
            "Good morning! May you walk into today with confidence, knowing that you are capable of handling more than you realize.",
            "Whatever you're building, don't give up because the results haven't appeared yet. Keep building. Good morning!",
            "Good morning! May today be kinder than yesterday, brighter than you expected, and more productive than you imagined.",
            "Good morning! May this new day bring you renewed strength, fresh opportunities, peace in your heart, and the courage to keep becoming everything you were created to be.",
            "Good morning! May your life continue to blossom like a garden planted by the Lord, and may your presence bring hope, peace, and fragrance wherever you go.",
            "Good morning! Remember, every sunrise is God's invitation to rise above yesterday's limitations and embrace today's possibilities with faith and courage.",
            "Good morning! You may not control the wind, but with God's wisdom, you can adjust your sails and navigate your way toward destiny.",
            "Good morning! The same God who paints the sky with beautiful colours has deposited gifts within you. Arise and let your life become a masterpiece.",
            "Good morning! Never underestimate the power of a renewed mind. With faith in God, a positive attitude, and consistent action, you can turn obstacles into opportunities.",
            "Good morning! You are not an accident, and your life is not without purpose. Walk boldly today, knowing that the Creator has a reason for your existence.",
            "Good morning! A seed does not become a mighty tree overnight. Be patient with your growth, faithful in your efforts, and confident in God's timing.",
            "Good morning! Your words can heal, your smile can encourage, and your kindness can restore hope. Make today beautiful by being a blessing to someone.",
            "Good morning! Do not allow yesterday's failures to become today's fears. Every new morning carries the grace to begin again and the wisdom to do better.",
            "Good morning! Greatness begins in the mind before it becomes visible in your hands. Think with wisdom, speak with faith, and act with purpose.",
            "Good morning! Like the eagle that rises above the storm, may you find the strength to rise above your worries and see your challenges from a higher perspective.",
            "Good morning! You carry more potential than your present circumstances reveal. Keep learning, keep growing, and never allow temporary struggles to define your permanent identity.",
            "Good morning! A heart filled with God's peace can remain steady in a world full of uncertainty. Guard your heart, renew your mind, and let faith guide your decisions.",
            "Good morning! Every act of obedience, every seed of kindness, and every effort made with a sincere heart contributes to a life of lasting impact. Never despise small beginnings.",
            "Good morning! Step into this beautiful day with the confidence that you are loved by God, equipped with a sound mind, and empowered to make meaningful contributions to your generation. Let your light shine!",
            "Good morning! You may not be able to change every situation around you, but you can change your response, strengthen your character, and grow through every experience.",
            "Good morning! The presence of challenges does not mean the absence of God. Keep believing, keep moving, and keep trusting Him even when the road seems unclear.",
            "Good morning! Let gratitude be your attitude and hope be your companion. When you appreciate what you have, you create room in your heart for greater possibilities.",
            "Good morning! Your life is a message that the world is reading. Let your integrity be the ink, your kindness the language, and your faith the inspiration.",
            "Good morning! Do not wait for perfect conditions before you take the first step. Start with what you have, use what you know, and trust God for the wisdom to continue."
        };

        public static string[] AfternoonGreetings = {
            "Good afternoon! Whatever the morning brought, you still have the rest of the day to make progress.",
            "Good afternoon! Take a deep breath, reset your mind, and keep moving forward.",
            "The day isn't over yet. There is still time for something beautiful to happen. Good afternoon!",
            "Good afternoon! May the second half of your day bring clarity, productivity, and unexpected good news.",
            "Don't let a slow morning convince you that the whole day is lost. Start again. Good afternoon!",
            "Good afternoon! Keep going. Sometimes the breakthrough comes after the moment you feel like giving up.",
            "Whatever didn't go according to plan this morning, don't carry the disappointment into the afternoon. Reset and continue.",
            "Good afternoon! May your efforts today produce results that make the waiting worthwhile.",
            "Half the day may be gone, but half the day is still ahead. Make it count!",
            "Good afternoon! You don't need a perfect day to make meaningful progress.",
            "May the rest of your day bring you peace where there has been pressure and solutions where there have been problems.",
            "Good afternoon! Keep your heart hopeful and your mind focused. Better moments may still be ahead.",
            "The afternoon is another reminder that you can pause, breathe, and begin again. Good afternoon!",
            "Good afternoon! May your hard work today become tomorrow's reason to celebrate.",
            "Don't measure your entire day by how it started. Sometimes the best part comes later.",
            "Good afternoon! Stay consistent. Small efforts repeated daily can create extraordinary results.",
            "May you find the strength to finish what you started and the wisdom to know what truly deserves your energy.",
            "Good afternoon! Whatever you're working toward, keep taking those small steps.",
            "A difficult morning doesn't cancel a promising afternoon. Keep believing. Good afternoon!",
            "Good afternoon! May the rest of today be filled with productive moments and peaceful thoughts.",
            "You have survived every difficult day that came before this one. Keep going. Good afternoon!",
            "Good afternoon! Don't allow temporary frustration to make permanent decisions for you.",
            "There is still time to turn things around. Stay focused and keep moving. Good afternoon!",
            "Good afternoon! May an opportunity find you when you least expect it.",
            "Sometimes progress is simply refusing to stop. Keep going this afternoon.",
            "Good afternoon! Your goals haven't disappeared just because today has been challenging.",
            "May the rest of your day remind you that patience and persistence are never wasted.",
            "Good afternoon! Focus on what you can do now instead of worrying about what you should have done earlier.",
            "The afternoon is a chance to adjust your plans, renew your energy, and continue with purpose.",
            "Good afternoon! May your remaining hours be more peaceful, productive, and rewarding than you imagined.",
            "Don't let one bad moment become the story of your entire day. Good afternoon!",
            "Good afternoon! Keep showing up. Consistency often accomplishes what motivation alone cannot.",
            "May your mind remain calm, your decisions wise, and your efforts fruitful this afternoon.",
            "Good afternoon! You may be tired, but remember why you started.",
            "There is still something good waiting to happen today. Keep your heart open. Good afternoon!",
            "Good afternoon! Take things one task at a time. You don't have to solve everything at once.",
            "May your afternoon bring answers to questions you've been carrying and peace to things you cannot control.",
            "Good afternoon! Don't underestimate what you can accomplish with a few focused hours.",
            "The day is still in your hands. Use the remaining hours wisely. Good afternoon!",
            "Good afternoon! May your determination be stronger than your distractions.",
            "When the day gets difficult, slow down—but don't give up. Good afternoon!",
            "Good afternoon! Your effort may not be visible yet, but that doesn't mean it isn't producing something.",
            "Keep planting, keep building, keep believing. Results often take time. Good afternoon!",
            "Good afternoon! May your next conversation bring encouragement, your next decision bring progress, and your next step bring you closer to your goal.",
            "Don't spend the afternoon regretting the morning. Use it to create a better evening.",
            "Good afternoon! A fresh perspective can turn an ordinary situation into a new opportunity.",
            "May you have the strength to finish strong and the grace to appreciate how far you've come.",
            "Good afternoon! Your journey doesn't have to look like anyone else's. Keep walking your path.",
            "There is no wasted day when you learn, grow, or move forward. Good afternoon!",
            "Good afternoon! May the remainder of your day bring you closer to something you've been hoping for.",
            "Stay encouraged. The work you're doing today may become the foundation for something much bigger tomorrow.",
            "Good afternoon! Don't let temporary circumstances determine your permanent outlook.",
            "May you find a reason to smile before this day ends. Good afternoon!",
            "Good afternoon! Sometimes all you need is a short pause, a clear mind, and another attempt.",
            "Your progress doesn't need to impress everyone. It simply needs to keep moving. Good afternoon!",
            "Good afternoon! May your remaining hours be filled with wisdom, favor, and meaningful progress.",
            "If today hasn't gone the way you hoped, there's still time to change how it ends. Good afternoon!",
            "Good afternoon! Stay patient. Some things take longer because they're worth building properly.",
            "Don't give up because you're tired. Rest if necessary, then continue. Good afternoon!",
            "Good afternoon! May you have clarity for your decisions and courage for your next move.",
            "The afternoon sun is a reminder that the day is still shining. Keep your hope alive.",
            "Good afternoon! One productive hour can change the direction of an entire day.",
            "May your afternoon be free from unnecessary worry and filled with things that truly matter.",
            "Good afternoon! You don't have to do everything today. Just make meaningful progress.",
            "Keep your vision ahead of you and your excuses behind you. Good afternoon!",
            "Good afternoon! May today's challenges become tomorrow's wisdom.",
            "Don't let disappointment steal the energy you need to create something better. Good afternoon!",
            "May you find unexpected strength for whatever remains on today's journey.",
            "Good afternoon! Keep working quietly. Not every seed needs to be seen before it grows.",
            "Your best moment today may still be ahead of you. Keep going. Good afternoon!",
            "Good afternoon! Choose peace over unnecessary arguments and purpose over unnecessary distractions.",
            "May your afternoon bring a fresh perspective and a renewed sense of purpose.",
            "Good afternoon! You have enough strength for the next step. Take it.",
            "Don't count the hours you've lost. Make the remaining hours count. Good afternoon!",
            "Good afternoon! May your efforts attract opportunities and your preparation meet the right moment.",
            "Whatever you're facing today, remember that difficult moments are temporary. Keep moving.",
            "Good afternoon! Let go of what you cannot change and put your energy into what you can.",
            "May this afternoon restore your confidence and remind you of what you're capable of achieving.",
            "Good afternoon! Progress may be slow, but slow progress is still progress.",
            "The day isn't finished writing its story. Keep showing up for your own story. Good afternoon!",
            "Good afternoon! May you finish today stronger, wiser, and more hopeful than you started.",
            "Don't allow a mistake from earlier today to stop you from making a better decision now.",
            "Good afternoon! Sometimes the greatest victory is simply choosing to keep going.",
            "May the rest of your day bring calm to your mind and joy to your heart.",
            "Good afternoon! Your future needs the effort you make today.",
            "Don't wait for tomorrow to correct what you can improve this afternoon. Start now.",
            "Good afternoon! May every challenge you face today teach you something that makes you stronger tomorrow.",
            "There is still room for good news, unexpected opportunities, and beautiful moments today. Good afternoon!",
            "Good afternoon! Keep your heart grateful even while you work toward something better.",
            "Good afternoon! May your afternoon be productive without stealing your peace.",
            "Good afternoon! You may be closer to your breakthrough than you realize. Keep going.",
            "Don't let tiredness make you forget your purpose. Rest, recharge, and continue.",
            "Good afternoon! May your remaining hours be guided by wisdom and filled with meaningful progress.",
            "Whatever you're building, give it patience. Great things rarely happen overnight.",
            "Good afternoon! You don't need to have everything figured out. Just take the next right step.",
            "May this afternoon bring a little more hope, a little more strength, and a lot more reasons to smile.",
            "Good afternoon! Keep believing in better possibilities while doing the work required to reach them.",
            "The morning may be gone, but your opportunity to make today meaningful remains. Good afternoon!",
            "Good afternoon! Finish the day with gratitude for what you have learned, pride in what you accomplished, and hope for what comes next.",
            "Good afternoon! Whatever today has brought so far, remember that the story is not finished. Keep your heart hopeful, your mind focused, and your steps steady. There is still time for something good to happen."
        };

        public static string[] EveningGreetings = {
            "Good evening! As the day comes to an end, remember that you did your best with what you had. Be proud of yourself.",
            "Good evening! Whatever today brought, tomorrow gives you another opportunity to try again.",
            "Let the evening remind you to slow down, breathe, and appreciate how far you've come. Good evening!",
            "Good evening! Not every day will be perfect, but every day can teach you something valuable.",
            "As the sun sets, release the worries of today and make room for the possibilities of tomorrow. Good evening!",
            "Good evening! You may not have accomplished everything today, but every honest effort counts.",
            "Take a moment tonight to appreciate the little victories that often go unnoticed. Good evening!",
            "Good evening! Don't carry today's disappointments into tomorrow. Learn from them, release them, and keep moving.",
            "The day is ending, but your journey continues. Rest tonight and return stronger tomorrow.",
            "Good evening! Sometimes the greatest progress is simply having the courage to keep going.",
            "May your evening bring peace to your mind and gratitude to your heart. Good evening!",
            "Good evening! Whatever didn't work today is not the end of the story. There is always another opportunity.",
            "Close the day with gratitude for what you have, wisdom from what you experienced, and hope for what lies ahead.",
            "Good evening! You survived every challenge that came your way today. Give yourself credit.",
            "Let go of what you cannot change tonight and focus on what tomorrow gives you the power to change.",
            "Good evening! Rest is not weakness. Sometimes stepping back is exactly what you need to move forward.",
            "May the quietness of this evening give you clarity for the decisions ahead. Good evening!",
            "Good evening! Don't judge your entire journey by one difficult day.",
            "Whatever happened today, don't forget that you are still growing, learning, and becoming better.",
            "Good evening! May tonight bring you the peace that a busy day could not give you.",
            "The sun has set, but hope doesn't have to. Keep believing in better days. Good evening!",
            "Good evening! Celebrate the progress you made today, no matter how small it may seem.",
            "Tonight, give yourself permission to rest without feeling guilty. Tomorrow needs a renewed version of you.",
            "Good evening! Your mistakes today can become the wisdom you need tomorrow.",
            "As this day ends, remember that setbacks are moments, not destinations. Good evening!",
            "May your heart be lighter tonight and your confidence stronger tomorrow. Good evening!",
            "Good evening! You don't need to have everything figured out tonight. Some answers come with time.",
            "Let tonight be a reminder that every ending creates space for a new beginning.",
            "Good evening! Be thankful for the lessons, the people, the opportunities, and even the challenges that shaped today.",
            "Whatever you're building, give it time. Good things often grow quietly. Good evening!",
            "Good evening! Don't let today's disappointments convince you that tomorrow won't be better.",
            "May tonight restore your energy and tomorrow renew your determination.",
            "Good evening! Sometimes you win, sometimes you learn. Either way, you move forward.",
            "Take a deep breath. The day's work is done. Give your mind permission to rest. Good evening!",
            "Good evening! Your worth isn't measured by how much you accomplished today.",
            "Tonight, be gentle with yourself. You're still becoming the person you want to be.",
            "Good evening! Tomorrow doesn't require you to be perfect—only willing to try again.",
            "May the peace of this evening replace the pressure of the day. Good evening!",
            "Good evening! If today didn't go as planned, don't lose hope. Plans can change, and opportunities can return.",
            "Before you sleep, remember at least one thing from today that deserves your gratitude.",
            "Good evening! The chapter of today may be closing, but your story is far from finished.",
            "Let go of the day's stress. Tomorrow is another canvas waiting for you. Good evening!",
            "Good evening! Your journey may be taking longer than expected, but that doesn't mean you're going in the wrong direction.",
            "May tonight remind you that you have made it through difficult seasons before. You will get through this one too.",
            "Good evening! Rest knowing that you don't have to solve every problem in one day.",
            "Don't let one bad experience erase all the good things that happened today. Good evening!",
            "Good evening! Be grateful for every small step that brought you closer to where you want to be.",
            "May your evening be peaceful, your thoughts be calm, and your heart be hopeful.",
            "Good evening! Some battles are won simply by refusing to give up.",
            "As the day closes, release the pressure to have everything figured out. Give yourself time to grow.",
            "Good evening! What matters is not whether today was perfect, but whether you kept moving.",
            "May tonight give you the strength to wake up tomorrow with renewed purpose.",
            "Good evening! Every experience carries a lesson if you're willing to see it.",
            "The day may be ending, but possibilities are never limited to today. Good evening!",
            "Good evening! Don't punish yourself for what you didn't know yesterday. Learn and move forward.",
            "May the quiet of tonight help you hear the wisdom that was difficult to hear during the busy day.",
            "Good evening! You are allowed to rest, reset, and start again.",
            "Leave today's worries where they belong—in today. Tomorrow deserves a fresh mind.",
            "Good evening! Sometimes the best thing you can accomplish at the end of a difficult day is finding peace.",
            "May your evening be filled with gratitude for what worked, acceptance of what didn't, and hope for what comes next.",
            "Good evening! Keep believing that your efforts will eventually produce something meaningful.",
            "Tonight, remember how many things once seemed impossible that you eventually overcame.",
            "Good evening! Don't compare your progress with someone else's timeline. Your journey has its own pace.",
            "May you find comfort in knowing that every day gives you another chance to improve.",
            "Good evening! The fact that you're still trying is already evidence of your strength.",
            "Let tonight be a pause, not a surrender. Rest and prepare to continue tomorrow.",
            "Good evening! You may have had a difficult day, but you don't have to have a difficult night.",
            "May your mind find peace, your body find rest, and your heart find hope tonight.",
            "Good evening! Sometimes tomorrow's breakthrough begins with tonight's decision not to give up.",
            "Don't carry anger, disappointment, or unnecessary worry into your rest. Protect your peace.",
            "Good evening! Be proud of the person who kept showing up even when things weren't easy.",
            "May tonight remind you that progress doesn't always look dramatic. Sometimes it looks like simply continuing.",
            "Good evening! Your future is still full of possibilities you haven't discovered yet.",
            "Take tonight to recharge, reflect, and prepare for another opportunity tomorrow.",
            "Good evening! The things you're working toward may take time, but patience doesn't mean you're standing still.",
            "May you sleep tonight knowing that you did what you could with the day you were given.",
            "Good evening! Don't allow today's failures to overshadow tomorrow's possibilities.",
            "Sometimes the strongest thing you can say at the end of a difficult day is, \"I'll try again tomorrow.\"",
            "Good evening! May your heart be free from yesterday's regrets and tomorrow's anxieties.",
            "Let the sunset remind you that even the busiest days eventually come to an end. Good evening!",
            "Good evening! You don't have to carry every burden alone. Allow yourself to pause and breathe.",
            "May tonight bring you perspective about the things that seemed overwhelming earlier today.",
            "Good evening! Every day you keep going, you become a little stronger than the person you were yesterday.",
            "Rest tonight without fear. You can face tomorrow one step at a time.",
            "Good evening! Don't underestimate the power of a peaceful mind and a rested body.",
            "May you end this day with more gratitude than regret and more hope than worry.",
            "Good evening! Sometimes life changes quietly before the results become visible. Keep trusting the process.",
            "Whatever you're waiting for, don't let the waiting steal your ability to enjoy today. Good evening!",
            "Good evening! There is something beautiful about knowing you can begin again tomorrow.",
            "Let tonight be a reminder that you are not defined by your worst moment, your biggest mistake, or your hardest season.",
            "Good evening! May the lessons of today become the wisdom that guides you tomorrow.",
            "The day is over. Give yourself permission to stop worrying about what you cannot change tonight.",
            "Good evening! You made it through today. That's something worth acknowledging.",
            "May tomorrow bring new opportunities, but tonight, give yourself the gift of rest.",
            "Good evening! Keep your dreams alive, even when the journey feels slower than you expected.",
            "Tonight, focus less on what went wrong and more on what you can do differently tomorrow.",
            "Good evening! Your story isn't finished because today was difficult. There are still better chapters ahead.",
            "May your evening be a peaceful bridge between everything you've experienced today and everything you're hoping for tomorrow.",
            "Good evening! Close today with gratitude, release what you cannot control, and prepare your heart for a fresh beginning.",
            "Good evening! As today comes to an end, may you find peace in what you've accomplished, wisdom in what you've learned, and hope for what tomorrow will bring. Rest well—you have another beautiful opportunity waiting for you."
        };

        public static string GetMorningGreeting()
        {
            return MorningGreetings[GenRandom.Next(MorningGreetings.Length)];
        }

        public static string GetAfternoonGreeting()
        {
            return AfternoonGreetings[GenRandom.Next(AfternoonGreetings.Length)];
        }

        public static string GetEveningGreeting()
        {
            return EveningGreetings[GenRandom.Next(EveningGreetings.Length)];
        }
    }
}
