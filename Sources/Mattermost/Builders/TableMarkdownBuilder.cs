using System;
using System.Text;
using Mattermost.Models.Enums;

namespace Mattermost.Builders
{
    /// <summary>
    /// Table markdown builder.
    /// </summary>
    public class TableMarkdownBuilder
    {
        private bool _isHeaderAdded;
        private readonly int _columns;
        private readonly StringBuilder _table;
        private readonly TableAlignment _tableAlignment;

        /// <summary>
        /// Create table builder.
        /// </summary>
        /// <param name="columns"> Columns count. </param>
        /// <param name="tableAlignment"> Table alignment, default is center. </param>
        /// <exception cref="ArgumentOutOfRangeException"> If columns count is less than or equal to zero. </exception>
        public TableMarkdownBuilder(int columns, TableAlignment tableAlignment = TableAlignment.Center)
        {
            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns), "Columns count must be greater than zero.");
            }
            _columns = columns;
            _tableAlignment = tableAlignment;
            _table = new StringBuilder();
        }

        /// <summary>
        /// Add header to table.
        /// </summary>
        /// <param name="headers"> Headers for columns. </param>
        /// <returns> Current builder. </returns>
        /// <exception cref="ArgumentNullException"> If headers count is not equal to columns count. </exception>
        public TableMarkdownBuilder AddHeader(params string[] headers)
        {
            if (headers == null || headers.Length != _columns)
            {
                throw new ArgumentNullException(nameof(headers), "Headers count must be equal to columns count.");
            }
            foreach (var header in headers)
            {
                _table.Append('|');
                _table.Append(header);
            }
            _table.Append('|');
            _table.AppendLine();
            _table.Append('|');
            for (int i = 0; i < _columns; i++)
            {
                switch (_tableAlignment)
                {
                    case TableAlignment.Left:
                        _table.Append(":---");
                        break;
                    case TableAlignment.Center:
                        _table.Append(":---:");
                        break;
                    case TableAlignment.Right:
                        _table.Append("---:");
                        break;
                }
                _table.Append('|');
            }
            _table.AppendLine();
            _isHeaderAdded = true;
            return this;
        }

        /// <summary>
        /// Add row to table.
        /// </summary>
        /// <param name="cells"> Cells in row. </param>
        /// <returns> Current builder. </returns>
        /// <exception cref="ArgumentNullException"> If cells count is not equal to columns count. </exception>
        /// <exception cref="InvalidOperationException"> If table header is not added. </exception>
        public TableMarkdownBuilder AddRow(params string[] cells)
        {
            if (cells == null || cells.Length != _columns)
            {
                throw new ArgumentNullException(nameof(cells), "Cells count must be equal to columns count.");
            }
            if (!_isHeaderAdded)
            {
                throw new InvalidOperationException("Table header is not added.");
            }

            foreach (var cell in cells)
            {
                _table.Append('|');
                _table.Append(cell);
            }
            _table.Append('|');
            _table.AppendLine();
            return this;
        }

        /// <summary>
        /// Add row to table.
        /// </summary>
        /// <param name="cells"> Cells in row. </param>
        /// <returns> Current builder. </returns>
        /// <exception cref="ArgumentNullException"> If cells count is not equal to columns count. </exception>
        /// <exception cref="InvalidOperationException"> If table header is not added. </exception>
        public TableMarkdownBuilder AddRow(params object[] cells)
        {
            if (cells == null || cells.Length != _columns)
            {
                throw new ArgumentNullException(nameof(cells), "Cells count must be equal to columns count.");
            }
            return AddRow(Array.ConvertAll(cells, x => x.ToString()));
        }

        /// <summary>
        /// Convert table to markdown string.
        /// </summary>
        /// <returns> Markdown string. </returns>
        /// <exception cref="InvalidOperationException"> If table header is not added. </exception>
        public override string ToString()
        {
            if (!_isHeaderAdded)
            {
                throw new InvalidOperationException("Table header is not added.");
            }
            return _table.ToString().Trim();
        }
    }
}
