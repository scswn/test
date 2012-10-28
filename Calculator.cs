using System;
using System.Collections.Generic;
using System.Text;

namespace VolumeFluctuation
{
    public class Calculator
    {
        private const int SLICE_TIME_DIVISION = 20;   // 0.05s = 1s / 20

        private int m_sample_rate;
        private int m_channel_num;
        private int m_sample_count_per_slice;

        private List<float> m_slice_sample_list;

        private int m_current_slice_sample_count = 0;
        private float m_current_slice_max_value = 0.0f;

        public void Init(int sample_rate, int channel_num)
        {
            m_sample_rate = sample_rate;
            m_channel_num = channel_num;

            m_sample_count_per_slice = m_sample_rate / SLICE_TIME_DIVISION;

            m_slice_sample_list = new List<float>();
        }

        public void Apply(byte[] buffer, int length)
        {
            int index = 0;
            float sample_value;

            while (index < length)
            {
                for (int i = 0; i < m_channel_num; i++)
                {
                    sample_value = Math.Abs(BitConverter.ToSingle(buffer, index));
                    if (sample_value > m_current_slice_max_value)
                    {
                        m_current_slice_max_value = sample_value;
                    }
                    index += sizeof(Single);
                }
                m_current_slice_sample_count++;
                if (m_current_slice_sample_count >= m_sample_count_per_slice)
                {
                    m_slice_sample_list.Add(m_current_slice_max_value);
                    m_current_slice_sample_count = 0;
                    m_current_slice_max_value = 0.0f;
                }
            }
        }

        private static float GetMaximumValue(List<float> values)
        {
            float max = 0.0f;

            foreach (float sample in values)
            {
                if (sample > max)
                {
                    max = sample;
                }
            }

            return max;
        }

        private static float GetAverageValue(List<float> values)
        {
            float sum = 0.0f;

            foreach (float value in values)
            {
                sum += value;
            }

            return sum / values.Count;
        }

        private static void NormalizeValues(ref List<float> values, float current_max, float target_max)
        {
            float scale = target_max / current_max;

            for (int i = 0; i < values.Count; i++)
            {
                values[i] *= scale;
            }
        }

        private static float GetStandardDeviation(List<float> values, float average_value)
        {
            // 因用于计算分段标准差时存在 sum(values) != N * average_value 的情况，此处不能使用简易公式进行计算

            float diff_square_sum = 0.0f;

            foreach (float value in values)
            {
                diff_square_sum += (value - average_value) * (value - average_value);
            }

            return (float)Math.Sqrt(diff_square_sum / values.Count);
        }

        private static string GetSegmentedStandardDeviations(List<float> values, float average_value, int segment_size)
        {
            List<float> segment_sds = new List<float>();

            int pos = 0;
            while (pos < values.Count) {
                List<float> segment_values = values.GetRange(
                    pos, Math.Min(segment_size, values.Count - pos));
                float diff = GetStandardDeviation(segment_values, average_value);
                segment_sds.Add(diff);
                pos += segment_size;
            }

            float segment_sds_avg = GetAverageValue(segment_sds);
            float segment_sds_sd = GetStandardDeviation(segment_sds, segment_sds_avg);

            StringBuilder sb = new StringBuilder();

            foreach (float diff_val in segment_sds)
            {
                sb.AppendFormat("{0:0.000} ", diff_val);
            }

            return String.Format("{0:0.000}] [{1}", segment_sds_sd, sb.ToString().TrimEnd(' '));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        private void DumpSamples(List<float> values, string filename)
        {
            System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create);
            System.IO.StreamWriter writer = new System.IO.StreamWriter(fs);
            foreach (float value in values)
            {
                writer.WriteLine("{0:0.000},", value);
            }
            writer.Close();
        }

        public void Calc()
        {
            List<float> samples = m_slice_sample_list;

            DumpSamples(samples, "dump_1.txt");

            float sample_max = GetMaximumValue(samples);

            NormalizeValues(ref samples, sample_max, 1.0f);

            DumpSamples(samples, "dump_2.txt");

            float sample_avg = GetAverageValue(samples);

            float sample_diff = GetStandardDeviation(samples, sample_avg);

            string seconds_diff = GetSegmentedStandardDeviations(samples, sample_avg, SLICE_TIME_DIVISION);

            Console.WriteLine("{0:0.000}, {1:0.000}, {2:0.000}, [{3}]",
                sample_max, sample_avg, sample_diff, seconds_diff);
        }
    }
}
