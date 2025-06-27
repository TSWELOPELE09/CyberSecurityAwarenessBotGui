using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CyberSecurityAwarenessBotGUI
{
    public partial class MainWindow : Window
    {
        private List<string> activityLog = new List<string>();
        private List<string> tasks = new List<string>();
        private List<QuizQuestion> quizQuestions = new List<QuizQuestion>();
        private int currentQuizIndex = 0;
        private int quizScore = 0;

        public MainWindow()
        {
            InitializeComponent();
            LoadQuizQuestions();
            PlayWelcomeAudio();
        }

        private void PlayWelcomeAudio()
        {
            string audioPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "welcome_message.wav");

            if (File.Exists(audioPath))
            {
                try
                {
                    SoundPlayer player = new SoundPlayer(audioPath);
                    player.Play();
                    LogActivity("Welcome audio played.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Audio error: " + ex.Message);
                }
            }
            else
            {
                LogActivity("Welcome audio file not found.");
            }
        }

        private void LogActivity(string action)
        {
            if (activityLog.Count >= 10)
            {
                activityLog.RemoveAt(0);
            }

            string timestamp = DateTime.Now.ToShortTimeString();
            string entry = string.Format("[{0}] {1}", timestamp, action);
            activityLog.Add(entry);

            ActivityLog.ItemsSource = null;
            ActivityLog.ItemsSource = activityLog;
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            string task = TaskInput.Text.Trim();
            if (!string.IsNullOrEmpty(task))
            {
                tasks.Add(task);
                TaskList.ItemsSource = null;
                TaskList.ItemsSource = tasks;

                LogActivity("Task added: " + task);
                MessageBox.Show("Task added: " + task);
                TaskInput.Clear();
            }
        }

        private void SendChat_Click(object sender, RoutedEventArgs e)
        {
            string input = ChatInput.Text.ToLower().Trim();
            if (string.IsNullOrEmpty(input)) return;

            ChatHistory.Items.Add("You: " + input);
            string response = GetBotResponse(input);
            ChatHistory.Items.Add("Bot: " + response);
            ChatInput.Clear();

            LogActivity("User asked: " + input);
        }

        private string GetBotResponse(string input)
        {
            if (input.Contains("password"))
                return "Use a strong password with numbers, symbols, and both upper and lower-case letters.";
            if (input.Contains("phishing"))
                return "Phishing is when scammers trick you into giving personal info. Don't click suspicious links.";
            if (input.Contains("privacy"))
                return "Keep your social media profiles private and avoid oversharing.";
            if (input.Contains("task"))
                return "You can add tasks using the Task Assistant on the right.";
            if (input.Contains("quiz"))
                return "Click 'Start Quiz' in the Quiz section.";
            if (input.Contains("log"))
                return "You can view your recent activity in the log section below.";

            return "I'm not sure I understand. Try asking about password safety, phishing, or privacy.";
        }

        // === QUIZ SECTION ===

        private void LoadQuizQuestions()
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion("What should you do if you receive a suspicious email asking for your password?", new List<string> { "A) Reply with password", "B) Delete it", "C) Report it as phishing", "D) Ignore it" }, "C"),
                new QuizQuestion("True or False: Using the same password for multiple accounts is safe.", new List<string> { "True", "False" }, "False"),
                new QuizQuestion("Which one is a good cybersecurity habit?", new List<string> { "A) Sharing passwords", "B) Using 2FA", "C) Clicking unknown links" }, "B"),
                new QuizQuestion("Phishing is:", new List<string> { "A) A security app", "B) Fake emails to steal info", "C) A type of virus" }, "B")
            };
        }

        private void StartQuiz_Click(object sender, RoutedEventArgs e)
        {
            currentQuizIndex = 0;
            quizScore = 0;
            ShowNextQuizQuestion();
            LogActivity("Quiz started.");
        }

        private void ShowNextQuizQuestion()
        {
            QuizOptions.Children.Clear();

            if (currentQuizIndex >= quizQuestions.Count)
            {
                QuizQuestion.Text = "Quiz complete! You scored " + quizScore + " out of " + quizQuestions.Count;
                QuizFeedback.Text = quizScore >= quizQuestions.Count * 0.75 ? "🎉 Great job!" : "📘 Keep learning to stay safe!";
                LogActivity("Quiz completed with score: " + quizScore);
                return;
            }

            QuizQuestion q = quizQuestions[currentQuizIndex];
            QuizQuestion.Text = q.Question;

            foreach (string option in q.Options)
            {
                RadioButton btn = new RadioButton();
                btn.Content = option;
                btn.Margin = new Thickness(2);
                QuizOptions.Children.Add(btn);
            }
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in QuizOptions.Children)
            {
                RadioButton rb = child as RadioButton;
                if (rb != null && rb.IsChecked == true)
                {
                    string selected = rb.Content.ToString();
                    string correct = quizQuestions[currentQuizIndex].Answer;

                    if (selected.StartsWith(correct))
                    {
                        QuizFeedback.Text = "✅ Correct!";
                        quizScore++;
                    }
                    else
                    {
                        QuizFeedback.Text = "❌ Incorrect. Correct answer is: " + correct;
                    }

                    currentQuizIndex++;
                    ShowNextQuizQuestion();
                    return;
                }
            }

            QuizFeedback.Text = "Please select an answer!";
        }
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public string Answer { get; set; }

        public QuizQuestion(string question, List<string> options, string answer)
        {
            Question = question;
            Options = options;
            Answer = answer;
        }
    }
}