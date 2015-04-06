using System;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

namespace FFMpegNet
{
    public class FFMPEGParameters
    {
        public string OutputFilePath;
        public string InputFilePath;
        public List<string> AdditionalFileInputs;
        public string Options;
        public string InputOptions;
        public string OutputOptions;
        public string VideoCodec;
        public string AudioCodec;
        public string Format;
        public Size Size = Size.Empty;
        public bool Overwrite;
        public bool QScale;
        public bool DisableAudio;

        public string ComplexVideoFilterInputs;
        public string ComplexVideoFilterCommands;
        public string VideoFilter;
        public string VideoProfile;
        public string Preset;

        public int? BufferSize;

        public int? MaximumRate;
        public int? MinimumRate;
        public int? VideoBitrate;
        public int? Pass;
        public int? AudioBitrate;
        public int? AudioChannels;
        public int? AudioRate;

        private StringBuilder m_assembledOptions;

        public FFMPEGParameters()
        {
            AdditionalFileInputs = new List<string>();
            m_assembledOptions = new StringBuilder();
        }

        protected void AddOption(string option)
        {
            if ((m_assembledOptions.Length > 0) && (m_assembledOptions.ToString().EndsWith(" ") == false))
            {
                m_assembledOptions.Append(" ");
            }

            m_assembledOptions.Append("-");
            m_assembledOptions.Append(option);
        }

        protected void AddParameter(string parameter)
        {
            m_assembledOptions.Append(parameter);
        }

        protected void AddOption(string option, string parameter)
        {
            AddOption(option);
            m_assembledOptions.Append(" ");
            AddParameter(parameter);
        }
        protected void AddComplexOption(string option, string inputs, string commands)
        {
            m_assembledOptions.Append(" ");
            AddParameter(inputs);
            AddOption(option);
            m_assembledOptions.Append(" \"");
            AddParameter(commands);
            m_assembledOptions.Append("\"");
        }

        protected void AddOption(string option, string parameter1, string separator, string parameter2)
        {
            AddOption(option);
            m_assembledOptions.Append(" ");
            AddParameter(parameter1);
            m_assembledOptions.Append(separator);
            AddParameter(parameter2);
        }

        protected void AddSeparator(string separator)
        {
            m_assembledOptions.Append(separator);
        }

        protected void AddRawOptions(string rawOptions)
        {
            m_assembledOptions.Append(rawOptions);
        }

        protected void AssembleGeneralOptions()
        {
            if (QScale)
            {
                AddOption("qscale 0");
            }

            if (Overwrite)
            {
                AddOption("y");
            }

            if (!String.IsNullOrWhiteSpace(Options))
            {
                AddSeparator(" ");
                AddRawOptions(Options);
            }

        }

        protected void AssembleInputOptions()
        {
            if (!String.IsNullOrWhiteSpace(InputOptions))
            {
                AddSeparator(" ");
                AddRawOptions(OutputOptions);
            }

        }

        protected void AssembleComplexOutputOptions()
        {
            if (!String.IsNullOrWhiteSpace(ComplexVideoFilterInputs))
            {
                AddComplexOption("filter_complex", ComplexVideoFilterInputs, ComplexVideoFilterCommands);
            }

        }

        protected void AssembleOutputOptions()
        {
            if (!String.IsNullOrWhiteSpace(VideoCodec))
            {
                AddOption("vcodec", VideoCodec);
            }

            if (!String.IsNullOrWhiteSpace(AudioCodec))
            {
                AddOption("acodec", AudioCodec);
            }

            if (!String.IsNullOrWhiteSpace(Format))
            {
                AddOption("f", Format);
            }

            if (BufferSize != null)
            {
                AddOption("bufsize", String.Format("{0}KB", BufferSize));
            }

            if (MaximumRate != null)
            {
                AddOption("maxrate", String.Format("{0}KB", MaximumRate));
            }

            if (MinimumRate != null)
            {
                AddOption("minrate", String.Format("{0}KB", MinimumRate));
            }

            if (VideoBitrate != null)
            {
                AddOption("b:v", String.Format("{0}k", VideoBitrate));
            }

            if (AudioBitrate != null)
            {
                AddOption("b:a", String.Format("{0}k", AudioBitrate));
            }

            if (AudioChannels != null)
            {
                AddOption("ac", AudioChannels.ToString());
            }

            if (AudioRate != null)
            {
                AddOption("ar", AudioRate.ToString());
            }

            if (Pass != null)
            {
                AddOption("pass", Pass.ToString());
            }

            if (!Size.IsEmpty)
            {
                AddOption("s", Size.Width.ToString(), "x", Size.Height.ToString());
            }

            if (!String.IsNullOrWhiteSpace(VideoProfile))
            {
                AddOption("vprofile", VideoProfile);
            }

            if (!String.IsNullOrWhiteSpace(Preset))
            {
                AddOption("preset", Preset);
            }


            if (!String.IsNullOrWhiteSpace(VideoFilter))
            {
                AddOption("vf", VideoFilter);
            }

            if (DisableAudio)
            {
                AddOption("an");
            }

            if (!String.IsNullOrWhiteSpace(OutputOptions))
            {
                AddSeparator(" ");
                AddRawOptions(OutputOptions);
            }

        }

        public override string ToString()
        {
            m_assembledOptions.Clear();
            AssembleGeneralOptions();
            AssembleInputOptions();
            if (!String.IsNullOrWhiteSpace(InputFilePath))
            {
                AddOption("i", String.Format("\"{0}\"", InputFilePath));
            }

            foreach (var file in AdditionalFileInputs)
            {
                if (!String.IsNullOrEmpty(file))
                    AddOption("i", String.Format("\"{0}\"", file));
            }


            AssembleOutputOptions();

            AssembleComplexOutputOptions();

            if (String.IsNullOrWhiteSpace(OutputFilePath))
            {
                AddSeparator(" ");
                AddParameter("NUL");
            }
            else
            {
                AddSeparator(" ");
                AddParameter(String.Format("\"{0}\"", OutputFilePath));
            }

            return m_assembledOptions.ToString();
        }
    }

}
